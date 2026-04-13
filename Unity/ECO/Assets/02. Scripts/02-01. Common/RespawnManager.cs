using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class RespawnManager : MonoBehaviourSingleton<RespawnManager>
{
    [SerializeField]
    private PlayerMotor _playerMotor;

    private Room _currentRoom;
    private List<Vector3> _currentSavePoints = new List<Vector3>();

    public Room CurrentRoom => _currentRoom;
    public IReadOnlyList<Vector3> CurrentSavePoints => _currentSavePoints;

    public void BindPlayer(PlayerMotor playerMotor)
    {
        _playerMotor = playerMotor;
    }

    public void UpdateSavePoint(Room room)
    {
        _currentRoom = room;
        _currentSavePoints.Clear();

        if (room != null && room.SpawnPoints != null)
        {
            for (int i = 0; i < room.SpawnPoints.Count; i++)
            {
                if (room.SpawnPoints[i] != null)
                {
                    _currentSavePoints.Add(room.SpawnPoints[i].position);
                }
            }
        }
    }

    [Button]
    public void Respawn()
    {
        if (_playerMotor != null && _currentSavePoints != null && _currentSavePoints.Count > 0)
        {
            int spawnIndex = GetRandomSpawnIndex();
            _playerMotor.Teleport(_currentSavePoints[spawnIndex]);
        }

        if (_currentRoom != null)
        {
            _currentRoom.ResetRoom();
        }

        // 추후 카메라 페이드 효과나 UI 갱신 등이 필요할 경우 로직 추가
    }

    private int GetRandomSpawnIndex()
    {
        return Random.Range(0, _currentSavePoints.Count);
    }
}
