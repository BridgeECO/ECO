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
    private CameraController _cameraController;

    public Room CurrentRoom => _currentRoom;
    public IReadOnlyList<Room> Rooms => _rooms;
    public ERegions RegionType => _regionType;

    private void Start()
    {
        InitCameraController();
        InitRegion();
    }

    private void InitCameraController()
    {
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.TryGetComponent<CameraController>(out _cameraController);
        }
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
            return;
        }
        InitFromDefaultSpawnPoint();
    }

    // 저장 데이터가 현재 씬의 Region과 일치하고 유효한 RoomIndex를 가지는지 검사한다.
    private bool IsValidContinueSaveData(SaveData saveData)
    {
        return saveData is not null && saveData.Region == _regionType && 0 <= saveData.RoomIndex && saveData.RoomIndex < _rooms.Count;
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
        if (_cameraController == null)
        {
            Debug.Log($"카메라 컨트롤러가 null입니다.");
            return;
        }
        _cameraController.SetRoomBounds(_currentRoom.MinBounds, _currentRoom.MaxBounds);
        _cameraController.transform.position = _cameraController.GetClampedPosition();
    }

    private void InitDefaultSavePoint()
    {
        if (_initialSpawnPoint == null)
        {
            Debug.LogWarning($"초기 스폰 포인트를 찾을 수 없습니다.");
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
        
        if (!isCameraTransitionSkipped)
        {
            EventManager.Instance.BroadcastEvent(EEventType.RoomChanged);
            return;
        }
        _cameraController.SetRoomBounds(_currentRoom.MinBounds, _currentRoom.MaxBounds);
    }

    public int GetRoomIndex(Room room)
    {
        return _rooms?.IndexOf(room) ?? -1;
    }
}