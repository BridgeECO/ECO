using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Region : MonoBehaviourSingleton<Region>
{
    // 저장된 좌표로 세이브 포인트를 되찾을 때 허용하는 반경.
    // 기획자가 세이브 포인트를 조금 옮겨도 기존 세이브가 깨지지 않도록 여유를 둔다.
    public const float SAVE_POINT_MATCH_DISTANCE = 2f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private ERegions _regionType;

    [SerializeField]
    private List<Room> _rooms;

    [SerializeField]
    private Transform _initialSpawnPoint;

    private Room _currentRoom;
    private CameraController _cameraController;
    private RegionSaveResolver _saveResolver;

    public Room CurrentRoom => _currentRoom;
    public IReadOnlyList<Room> Rooms => _rooms;
    public ERegions RegionType => _regionType;

    private void Start()
    {
        if (_rooms == null || _rooms.Count == 0)
        {
            Debug.LogError($"[Region] {name}에 Room이 하나도 등록되지 않아 초기화를 중단합니다.");
            return;
        }

        _saveResolver = new RegionSaveResolver(_rooms, _regionType);
        InitCameraController();
        InitRoomIndices();
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

    // 세이브 포인트가 진행 순서를 스스로 판정할 수 있도록 각 방에 목록 순서를 주입한다.
    private void InitRoomIndices()
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            if (_rooms[i] != null)
            {
                _rooms[i].InitIndex(i);
            }
        }
    }

    private void InitRegion()
    {
        SaveData saveData = SaveManager.Instance.CurrentSaveData;

        // 이어하기로 진입한 경우: 저장된 좌표에 해당하는 세이브 포인트를 찾아 그 방으로 초기화한다.
        if (_saveResolver.IsValidContinueSaveData(saveData))
        {
            SavePoint savePoint = _saveResolver.FindSavePointAt(saveData.SavePointPosition);
            if (savePoint != null && savePoint.OwnerRoom != null)
            {
                InitFromSavePoint(savePoint);
                return;
            }
            Debug.LogWarning($"저장된 좌표 {saveData.SavePointPosition}에 해당하는 세이브 포인트를 찾지 못해 기본 스폰 지점에서 시작합니다.");
        }
        InitFromDefaultSpawnPoint();
    }

    private void InitFromSavePoint(SavePoint savePoint)
    {
        _currentRoom = savePoint.OwnerRoom;
        InitCameraBounds();

        // 저장된 좌표가 아니라 현재 세이브 포인트의 좌표를 쓴다.
        // 기획자가 세이브 포인트를 옮겼을 때 플레이어가 허공에 놓이는 것을 막는다.
        BroadcastRegionInitialized(new RegionSpawnInfo(savePoint, _currentRoom, savePoint.RespawnPosition));
    }

    // 새 게임 진입 지점. 저장은 하지 않는다 — 첫 세이브는 세이브 포인트를 통과할 때 기록된다.
    private void InitFromDefaultSpawnPoint()
    {
        // 목록 앞쪽에 빈 슬롯이 남아 있어도 초기화가 깨지지 않도록 첫 유효 방을 찾는다.
        _currentRoom = FindFirstValidRoom();
        if (_currentRoom == null)
        {
            Debug.LogError($"[Region] {name}의 Room 목록에 유효한 Room이 없어 초기화를 중단합니다.");
            return;
        }
        InitCameraBounds();

        if (_initialSpawnPoint == null)
        {
            Debug.LogWarning($"초기 스폰 포인트를 찾을 수 없습니다.");
            return;
        }
        BroadcastRegionInitialized(new RegionSpawnInfo(null, _currentRoom, _initialSpawnPoint.position));
    }

    private Room FindFirstValidRoom()
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            if (_rooms[i] != null)
            {
                return _rooms[i];
            }
        }
        return null;
    }

    // 초기 배치 결과를 이벤트로 발행한다. 실제 리스폰 지점 갱신과 플레이어 이동은
    // RespawnManager가 구독해 수행한다. (Region→RespawnManager 역방향 호출 제거)
    private void BroadcastRegionInitialized(RegionSpawnInfo info)
    {
        EventManager.Instance.BroadcastEvent(EEventType.RegionInitialized, info);
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

    public void SetCurrentRoom(Room newRoom, bool isCameraTransitionSkipped = false)
    {
        if (_currentRoom == newRoom)
        {
            return;
        }

        _currentRoom = newRoom;

        // 페이로드 채널: 방 상태 변화 통지 (BGM 정책 등) — 카메라 전환 여부와 무관하게 항상 발행
        EventManager.Instance.BroadcastEvent(EEventType.RoomChanged, newRoom);

        if (!isCameraTransitionSkipped)
        {
            // 파라미터리스 채널: 카메라 전환 연출 트리거
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
}
