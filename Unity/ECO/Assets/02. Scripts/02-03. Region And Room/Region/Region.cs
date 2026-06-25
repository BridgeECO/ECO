using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Region : MonoBehaviourSingleton<Region>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private ERegions _regionType;

    [SerializeField]
    private List<Room> _rooms;

    [SerializeField]
    private Transform _initialSpawnPoint;

    private Room _currentRoom;

    public Room CurrentRoom => _currentRoom;
    public IReadOnlyList<Room> Rooms => _rooms;
    public ERegions RegionType => _regionType;

    private void Start()
    {
        InitRegion();
    }

    private void InitRegion()
    {
        if (_rooms == null || _rooms.Count == 0)
        {
            return;
        }

        SaveData saveData = SaveManager.Instance.CurrentSaveData;

        // 이어하기로 진입한 경우: 저장된 Region과 일치하고 유효한 RoomIndex가 있으면 해당 방/위치로 초기화
        if (IsValidContinueSaveData(saveData))
        {
            InitFromSaveData(saveData);
        }
        else
        {
            InitFromDefaultSpawnPoint();
        }
    }

    /// <summary>
    /// 저장 데이터가 현재 씬의 Region과 일치하고 유효한 RoomIndex를 가지는지 검사한다.
    /// </summary>
    private bool IsValidContinueSaveData(SaveData saveData)
    {
        if (saveData is null)
        {
            return false;
        }
        if (saveData.Region != _regionType)
        {
            return false;
        }
        if (saveData.RoomIndex < 0 || saveData.RoomIndex >= _rooms.Count)
        {
            return false;
        }
        return true;
    }

    private void InitFromSaveData(SaveData saveData)
    {
        _currentRoom = _rooms[saveData.RoomIndex];
        _currentRoom.IsVisited = true;
        InitCameraBounds();

        // UpdateSavePoint는 세이브포인트 등록과 파일 저장을 담당하고,
        // 이어하기 진입 시에는 플레이어를 해당 위치로 즉시 텔레포트한다.
        RespawnManager.Instance.UpdateSavePoint(_currentRoom, saveData.SavePointPosition);
        RespawnManager.Instance.TeleportPlayer(saveData.SavePointPosition);
    }

    private void InitFromDefaultSpawnPoint()
    {
        _currentRoom = _rooms[0];
        _currentRoom.IsVisited = true;
        InitCameraBounds();
        InitDefaultSavePoint();
    }

    private void InitCameraBounds()
    {
        var mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.TryGetComponent<CameraController>(out var cameraController))
        {
            cameraController.SetRoomBounds(_currentRoom.MinBounds, _currentRoom.MaxBounds);
            cameraController.transform.position = cameraController.GetClampedPosition();
        }
    }

    private void InitDefaultSavePoint()
    {
        if (_initialSpawnPoint == null)
        {
            Debug.LogWarning("초기 스폰 포인트를 찾을 수 없습니다.");
            return;
        }
        RespawnManager.Instance.UpdateSavePoint(_currentRoom, _initialSpawnPoint.position);
        RespawnManager.Instance.TeleportPlayer(_initialSpawnPoint.position);
    }

    public void SetCurrentRoom(Room newRoom, Vector3 spawnPosition, bool isCameraTransitionSkipped = false)
    {
        if (_currentRoom == newRoom)
        {
            return;
        }

        _currentRoom = newRoom;
        _currentRoom.IsVisited = true;
        RespawnManager.Instance.UpdateSavePoint(_currentRoom, spawnPosition);
        LifeManager.Instance.RecoverToRoomTransition();
        
        if (isCameraTransitionSkipped)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.TryGetComponent<CameraController>(out var cameraController))
            {
                cameraController.SetRoomBounds(_currentRoom.MinBounds, _currentRoom.MaxBounds);
            }
        }
        else
        {
            EventManager.Instance.BroadcastEvent(EEventType.RoomChanged);
        }
    }

    public int GetRoomIndex(Room room)
    {
        if (_rooms == null)
        {
            return -1;
        }

        for (int i = 0; i < _rooms.Count; i++)
        {
            if (_rooms[i] == room)
            {
                return i;
            }
        }
        return -1;
    }
}