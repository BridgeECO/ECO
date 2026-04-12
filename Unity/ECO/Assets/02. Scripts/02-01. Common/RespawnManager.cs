using UnityEngine;
using VInspector;

public class RespawnManager : MonoBehaviourSingleton<RespawnManager>
{
    [SerializeField]
    private PlayerMotor _playerMotor;

    private Room _currentRoom;
    private Vector3 _currentSavePoint;

    public Room CurrentRoom => _currentRoom;
    public Vector3 CurrentSavePoint => _currentSavePoint;

    public void BindPlayer(PlayerMotor playerMotor)
    {
        _playerMotor = playerMotor;
    }

    public void UpdateSavePoint(Room room, Vector3 savePoint)
    {
        _currentRoom = room;
        _currentSavePoint = savePoint;
    }

    [Button]
    public void Respawn()
    {
        if (_playerMotor != null)
        {
            _playerMotor.Teleport(_currentSavePoint);
        }

        if (_currentRoom != null)
        {
            _currentRoom.ResetRoom();
        }

        // 추후 카메라 페이드 효과나 UI 갱신 등이 필요할 경우 로직 추가

    }
}
