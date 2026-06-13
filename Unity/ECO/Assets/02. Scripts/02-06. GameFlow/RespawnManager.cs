using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class RespawnManager : MonoBehaviourSingleton<RespawnManager>
{
    private PlayerMotor _playerMotor;
    private Room _saveRoom;
    private Vector3 _respawnPosition;
    public Vector3 RespawnPosition => _respawnPosition;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        EventManager.Instance.AddEventListener(EEventType.PlayerDied, Respawn);
        BindPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, Respawn);
        }
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
        _saveRoom = room;
        _respawnPosition = position;
        SaveManager.Instance.Save(room);
    }

    public void Respawn()
    {
        MovePlayerToSavePoint();
        ResetCurrentRoom();
        EventManager.Instance.BroadcastEvent(EEventType.PlayerRespawned);
    }

    private void MovePlayerToSavePoint()
    {
        if (_playerMotor == null)
        {
            Debug.LogWarning("세이브 포인트로 이동시킬 대상(플레이어가)을 찾을 수 없습니다.");
            return;
        }
        _playerMotor.Teleport(_respawnPosition);
    }

    private void ResetCurrentRoom()
    {
        if (_saveRoom == null)
        {
            Debug.LogWarning("기믹 상태를 리셋할 방을 찾을 수 없습니다.");
            return;
        }
        _saveRoom.ResetRoom();
    }

}
