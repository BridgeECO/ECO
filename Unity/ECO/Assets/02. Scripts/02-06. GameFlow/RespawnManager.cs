using UnityEngine;
using VInspector;
using Cysharp.Threading.Tasks;

/// <summary>
/// 플레이어 사망 시 세이브포인트로 리스폰하는 책임을 담당한다.
/// PersistentScene에 플레이어와 함께 상주하므로 PlayerMotor를 인스펙터에서 직접 바인딩한다.
/// </summary>
public class RespawnManager : MonoBehaviourSingleton<RespawnManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private PlayerMotor _playerMotor;

    private Room _saveRoom;
    private Vector3 _respawnPosition;
    public Vector3 RespawnPosition => _respawnPosition;

    private void Start()
    {
        EventManager.Instance.AddEventListener(EEventType.PlayerDied, Respawn);
    }

    private void OnDisable()
    {
        RemoveEventListeners();
    }

    private void RemoveEventListeners()
    {
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, Respawn);
        }
    }

    public void UpdateSavePoint(Room room, Vector3 position)
    {
        _saveRoom = room;
        _respawnPosition = position;
        SaveManager.Instance.Save(room, position);
    }

    /// <summary>
    /// 이어하기 진입 시 리스폰 이벤트 없이 플레이어를 세이브포인트 위치로 즉시 이동시킨다.
    /// </summary>
    public void TeleportPlayer(Vector3 position)
    {
        MovePlayer(position);
    }

    public void Respawn()
    {
        RespawnAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid RespawnAsync(System.Threading.CancellationToken cancellationToken)
    {
        InputHandler.BlockInput();
        try
        {
            if (UIManager.Instance != null)
            {
                await UIManager.Instance.FadeOutAsync(1f, cancellationToken);
            }

            MovePlayer(_respawnPosition);
            ResetCurrentRoom();

            if (UIManager.Instance != null)
            {
                await UIManager.Instance.FadeInAsync(1f, cancellationToken);
            }

            EventManager.Instance.BroadcastEvent(EEventType.PlayerRespawned);
        }
        finally
        {
            InputHandler.UnblockInput();
        }
    }

    private void MovePlayer(Vector3 position)
    {
        if (_playerMotor == null)
        {
            Debug.LogWarning("세이브 포인트로 이동시킬 대상(플레이어)을 찾을 수 없습니다.");
            return;
        }
        _playerMotor.Teleport(position);
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
