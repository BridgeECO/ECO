using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Region : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private List<Room> _rooms;

    private Room _currentRoom;

    public IReadOnlyList<Room> Rooms => _rooms;

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
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.UpdateSavePoint(_currentRoom);
        }
    }
}