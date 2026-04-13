using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Region : MonoBehaviourSingleton<Region>
{
    [Foldout("Project")]
    [SerializeField]
    private ERegions _regionType;

    [SerializeField]
    private List<Room> _rooms;

    private Room _currentRoom;

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
        RespawnManager.Instance?.UpdateSavePoint(_currentRoom);
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