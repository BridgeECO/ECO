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
        _currentRoom = _rooms[0];
        _currentRoom.IsVisited = true;
        InitCameraBounds();
        InitSavePoint();
    }

    private void InitCameraBounds()
    {
        if (Camera.main != null)
        {
            CameraController cameraController = Camera.main.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.SetRoomBounds(_currentRoom.MinBounds, _currentRoom.MaxBounds);
                cameraController.transform.position = cameraController.GetClampedPosition();
            }
        }
    }

    private void InitSavePoint()
    {
        if (_initialSpawnPoint == null)
        {
            Debug.LogWarning("초기 스폰 포인트를 찾을 수 없습니다.");
            return;
        }
        RespawnManager.Instance.UpdateSavePoint(_currentRoom, _initialSpawnPoint.position);
    }

    public void SetCurrentRoom(Room newRoom, Vector3 spawnPosition)
    {
        if (_currentRoom == newRoom)
        {
            return;
        }

        _currentRoom = newRoom;
        _currentRoom.IsVisited = true;
        RespawnManager.Instance.UpdateSavePoint(_currentRoom, spawnPosition);
        LifeManager.Instance.RecoverToRoomTransition();
        EventManager.Instance.BroadcastEvent(EEventType.RoomChanged);
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