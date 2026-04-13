using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class RespawnManager : MonoBehaviourSingleton<RespawnManager>
{
    private PlayerMotor _playerMotor;
    private Room _currentRoom;
    private Vector3 _respawnPosition;

    public Room CurrentRoom => _currentRoom;
    public Vector3 RespawnPosition => _respawnPosition;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        BindPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindPlayer();
    }

    private void BindPlayer()
    {
        _playerMotor = FindAnyObjectByType<PlayerMotor>();
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
