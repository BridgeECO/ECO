using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Region : MonoBehaviourSingleton<Region>
{
    // 저장된 좌표로 세이브 포인트를 되찾을 때 허용하는 반경.
    // 기획자가 세이브 포인트를 조금 옮겨도 기존 세이브가 깨지지 않도록 여유를 둔다.
    public const float SAVE_POINT_MATCH_DISTANCE = 2f;
    public const int INVALID_ROOM_INDEX = -1;

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
            _cameraController = mainCamera.GetComponentInParent<CameraController>();
        }
    }

    private void InitRegion()
    {
        if (_rooms == null || _rooms.Count == 0)
        {
            return;
        }

        SaveData saveData = SaveManager.Instance.CurrentSaveData;

        // 이어하기로 진입한 경우: 저장된 좌표에 해당하는 세이브 포인트를 찾아 그 방으로 초기화한다.
        if (IsValidContinueSaveData(saveData))
        {
            SavePoint savePoint = GetSavePointAt(saveData.SavePointPosition);
            if (savePoint != null && savePoint.OwnerRoom != null)
            {
                InitFromSavePoint(savePoint);
                return;
            }
            Debug.LogWarning($"저장된 좌표 {saveData.SavePointPosition}에 해당하는 세이브 포인트를 찾지 못해 기본 스폰 지점에서 시작합니다.");
        }
        InitFromDefaultSpawnPoint();
    }

    // 저장 데이터가 현재 씬의 Region과 일치하는지 검사한다.
    private bool IsValidContinueSaveData(SaveData saveData)
    {
        return saveData is not null && saveData.Region == _regionType;
    }

    private void InitFromSavePoint(SavePoint savePoint)
    {
        _currentRoom = savePoint.OwnerRoom;
        InitCameraBounds();

        // 저장된 좌표가 아니라 현재 세이브 포인트의 좌표를 쓴다.
        // 기획자가 세이브 포인트를 옮겼을 때 플레이어가 허공에 놓이는 것을 막는다.
        RespawnManager.Instance.SetRespawnPoint(savePoint);
        RespawnManager.Instance.TeleportPlayer(savePoint.RespawnPosition);
    }

    private void InitFromDefaultSpawnPoint()
    {
        _currentRoom = _rooms[0];
        InitCameraBounds();
        InitDefaultRespawnPoint();
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

    // 새 게임 진입 지점. 저장은 하지 않는다 — 첫 세이브는 세이브 포인트를 통과할 때 기록된다.
    private void InitDefaultRespawnPoint()
    {
        if (_initialSpawnPoint == null)
        {
            Debug.LogWarning($"초기 스폰 포인트를 찾을 수 없습니다.");
            return;
        }
        RespawnManager.Instance.SetRespawnPoint(_currentRoom, _initialSpawnPoint.position);
        RespawnManager.Instance.TeleportPlayer(_initialSpawnPoint.position);
    }

    public void SetCurrentRoom(Room newRoom, bool isCameraTransitionSkipped = false)
    {
        if (_currentRoom == newRoom)
        {
            return;
        }

        _currentRoom = newRoom;

        if (newRoom.RoomType == ERoomType.Boss)
        {
            SoundManager.Instance.StopBgm();
        }
        else
        {
            SoundManager.Instance.PlayBgm(EBgmType.SyrNormal);
        }

        if (!isCameraTransitionSkipped)
        {
            EventManager.Instance.BroadcastEvent(EEventType.RoomChanged);
            return;
        }
        _cameraController.SetRoomBounds(_currentRoom.MinBounds, _currentRoom.MaxBounds);
    }

    /// <summary>
    /// 리스폰 시 세이브 포인트가 속한 방으로 되돌린다.
    /// RoomChanged를 브로드캐스트하지 않아 페이드 도중 카메라 전환 연출이 끼어들지 않는다.
    /// </summary>
    public void SetCurrentRoomOnRespawn(Room saveRoom)
    {
        if (saveRoom == null || _currentRoom == saveRoom)
        {
            return;
        }
        _currentRoom = saveRoom;
        InitCameraBounds();
    }

    // 세이브 포인트의 진행 순서를 판정하는 1차 기준. 목록에 없으면 INVALID_ROOM_INDEX.
    public int GetRoomIndex(Room room)
    {
        if (room == null)
        {
            return INVALID_ROOM_INDEX;
        }
        return _rooms.IndexOf(room);
    }

    /// <summary>
    /// 저장된 좌표에 해당하는 세이브 포인트를 찾는다.
    /// 허용 반경 안에서 가장 가까운 것을 반환하며, 없으면 null.
    /// </summary>
    public SavePoint GetSavePointAt(Vector3 position)
    {
        SavePoint nearestSavePoint = null;
        float nearestSqrDistance = SAVE_POINT_MATCH_DISTANCE * SAVE_POINT_MATCH_DISTANCE;

        for (int i = 0; i < _rooms.Count; i++)
        {
            if (_rooms[i] == null)
            {
                continue;
            }

            // 비활성 Room은 Awake가 실행되지 않아 목록이 null일 수 있다.
            IReadOnlyList<SavePoint> savePoints = _rooms[i].SavePoints;
            if (savePoints == null)
            {
                continue;
            }

            for (int j = 0; j < savePoints.Count; j++)
            {
                float sqrDistance = ((Vector2)(savePoints[j].RespawnPosition - position)).sqrMagnitude;
                if (nearestSqrDistance <= sqrDistance)
                {
                    continue;
                }
                nearestSqrDistance = sqrDistance;
                nearestSavePoint = savePoints[j];
            }
        }
        return nearestSavePoint;
    }
}
