using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class RespawnManager : MonoBehaviourSingleton<RespawnManager>
{
    [SerializeField]
    private PlayerMotor _playerMotor;

    private Room _currentRoom;
    private Vector3 _respawnPosition;

    public Room CurrentRoom => _currentRoom;
    public Vector3 RespawnPosition => _respawnPosition;

    public void BindPlayer(PlayerMotor playerMotor)
    {
        _playerMotor = playerMotor;
    }

    public void UpdateSavePoint(Room room, Vector3 position)
    {
        _currentRoom = room;
        _respawnPosition = position;
    }

    [Button]
    public void Respawn()
    {
        if (_playerMotor != null)
        {
            _playerMotor.Teleport(_respawnPosition);
        }

        if (_currentRoom != null)
        {
            _currentRoom.ResetRoom();
        }
    }
}
