using System.Collections.Generic;
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

    private SavePoint _currentSavePoint;
    private Room _saveRoom;
    private Vector3 _respawnPosition;

    public Vector3 RespawnPosition => _respawnPosition;

    // 리스폰 텔레포트가 플레이어를 세이브 포인트 트리거 위로 옮기기 때문에
    // 페이드 도중 OnTriggerEnter2D가 재발동한다. SavePoint가 이 플래그로 자신을 억제한다.
    public bool IsRespawning { get; private set; }

    private void OnEnable()
    {
        if (EventManager.Instance == null)
        {
            return;
        }
        EventManager.Instance.AddEventListener(EEventType.PlayerDied, Respawn);
        EventManager.Instance.AddEventListener<RegionSpawnInfo>(EEventType.RegionInitialized, OnRegionInitialized);
        EventManager.Instance.AddEventListener<SavePoint>(EEventType.SavePointReached, OnSavePointReached);
    }

    private void OnDisable()
    {
        RemoveEventListeners();
    }

    private void RemoveEventListeners()
    {
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, Respawn);
            EventManager.Instance.RemoveEventListener<RegionSpawnInfo>(EEventType.RegionInitialized, OnRegionInitialized);
            EventManager.Instance.RemoveEventListener<SavePoint>(EEventType.SavePointReached, OnSavePointReached);
        }
    }

    // Region이 초기 배치를 결정하면 리스폰 지점을 갱신하고 플레이어를 이동시킨다.
    private void OnRegionInitialized(RegionSpawnInfo info)
    {
        if (info.SavePoint != null)
        {
            SetRespawnPoint(info.SavePoint);
        }
        else
        {
            SetRespawnPoint(info.Room, info.Position);
        }
        TeleportPlayer(info.Position);
    }

    // 세이브 포인트 통과 이벤트를 수락할지 판단하고, 수락 시 사용 처리·저장·라이프 회복을 수행한다.
    private void OnSavePointReached(SavePoint savePoint)
    {
        // 리스폰은 플레이어를 세이브 포인트 트리거 위로 텔레포트시키므로 페이드 도중 재발동한다.
        // 막지 않으면 리스폰 라이프(2개)가 즉시 최대치로 덮어써져 하트 연출이 튄다.
        if (IsRespawning)
        {
            return;
        }

        savePoint.SetUsed();
        SetSavePoint(savePoint);
        if (LifeManager.Instance != null)
        {
            LifeManager.Instance.SetLifeToMax();
        }
    }

    /// <summary>
    /// 세이브 포인트 통과 시 리스폰 지점을 갱신하고 저장 파일을 기록한다.
    /// 진행 순서상 앞선 세이브 포인트로만 갱신된다.
    /// </summary>
    private void SetSavePoint(SavePoint savePoint)
    {
        // 직전과 같은 세이브 포인트면 기록될 내용이 동일하므로 파일 쓰기를 생략한다.
        if (savePoint == null || _currentSavePoint == savePoint)
        {
            return;
        }

        // 되돌아가 이전 세이브 포인트를 밟아도 진행도가 뒤로 밀리지 않게 한다.
        if (!savePoint.IsAheadOf(_currentSavePoint))
        {
            return;
        }

        // 파일 기록에 실패해도 리스폰 지점은 갱신한다. 저장은 실패했더라도
        // 이번 세션에서 죽었을 때 방금 지난 지점으로 되살아나는 편이 낫다.
        SetRespawnPoint(savePoint);

        // 저장되지 않았는데 갱신 알림을 띄우면 플레이어에게 거짓말이 된다.
        if (!TrySaveProgress())
        {
            return;
        }
        EventManager.Instance.BroadcastEvent(EEventType.SavePointUpdated);
    }

    // 저장에 필요한 컨텍스트(씬 이름, 지역)를 수집해 SaveManager에 전달한다.
    // SaveManager가 Region/SceneTransitionManager를 직접 조회하지 않도록 여기서 모은다.
    private bool TrySaveProgress()
    {
        if (Region.Instance == null || SceneTransitionManager.Instance == null)
        {
            return false;
        }

        string activeSceneName = SceneTransitionManager.Instance.CurrentLoadedRegionScene;
        if (string.IsNullOrEmpty(activeSceneName) || !System.Enum.TryParse(activeSceneName, out ESceneNames sceneName))
        {
            // TitleScene으로 폴백하면 이어하기가 타이틀로 진입하는 슬롯이 만들어지므로 저장 자체를 중단한다.
            Debug.LogError($"현재 씬 이름('{activeSceneName}')을 ESceneNames로 변환할 수 없어 저장을 건너뜁니다.");
            return false;
        }

        return SaveManager.Instance.Save(sceneName, Region.Instance.RegionType, _respawnPosition);
    }

    /// <summary>
    /// 저장 없이 리스폰 지점만 갱신한다. 이어하기 진입 시 사용한다.
    /// </summary>
    private void SetRespawnPoint(SavePoint savePoint)
    {
        if (savePoint == null)
        {
            return;
        }
        _currentSavePoint = savePoint;
        _saveRoom = savePoint.OwnerRoom;
        _respawnPosition = savePoint.RespawnPosition;
    }

    /// <summary>
    /// 세이브 포인트 없이 리스폰 지점을 지정한다. 새 게임의 기본 스폰 지점에서 사용한다.
    /// Region 진입마다 반드시 호출되므로, 씬 언로드로 파괴된 세이브 포인트 참조를 여기서 비운다.
    /// </summary>
    private void SetRespawnPoint(Room room, Vector3 position)
    {
        _currentSavePoint = null;
        _saveRoom = room;
        _respawnPosition = position;
    }

    /// <summary>
    /// 이어하기 진입 시 리스폰 이벤트 없이 플레이어를 세이브포인트 위치로 즉시 이동시킨다.
    /// </summary>
    private void TeleportPlayer(Vector3 position)
    {
        MovePlayer(position);
    }

    public void Respawn()
    {
        RespawnAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid RespawnAsync(System.Threading.CancellationToken cancellationToken)
    {
        IsRespawning = true;
        InputHandler.BlockInput();
        try
        {
            if (UIManager.Instance != null)
            {
                await UIManager.Instance.FadeOutAsync(1f, cancellationToken);
            }

            EventManager.Instance.BroadcastEvent(EEventType.RespawnReset);

            MovePlayer(_respawnPosition);
            RestoreCurrentRoom();
            ResetRegionRooms();

            if (LifeManager.Instance != null)
            {
                LifeManager.Instance.SetLifeOnRespawn();
            }

            // 페이드인 직전에 방 복원 완료를 알린다. BGM 재개는 BgmDirector가 구독해 처리한다.
            EventManager.Instance.BroadcastEvent(EEventType.RespawnStateRestored, _saveRoom);

            if (UIManager.Instance != null)
            {
                await UIManager.Instance.FadeInAsync(1f, cancellationToken);
            }

            EventManager.Instance.BroadcastEvent(EEventType.PlayerRespawned);
        }
        finally
        {
            IsRespawning = false;
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

    // 세이브 포인트가 아닌 다른 방에서 사망하면 카메라 바운드가 사망 지점에 남으므로 되돌린다.
    // Teleport는 rigidbody 위치를 쓰고 카메라는 transform 위치를 읽으므로 MovePlayer 이후에 호출한다.
    private void RestoreCurrentRoom()
    {
        if (_saveRoom == null || Region.Instance == null)
        {
            return;
        }
        Region.Instance.SetCurrentRoomOnRespawn(_saveRoom);
    }

    // 세이브 포인트가 Room에서 분리되면서 사망한 방이 리셋되지 않는 구멍이 생기므로
    // Region의 모든 방을 세이브 시점 상태로 되돌린다.
    private void ResetRegionRooms()
    {
        if (Region.Instance == null)
        {
            Debug.LogWarning("기믹 상태를 리셋할 Region을 찾을 수 없습니다.");
            return;
        }

        IReadOnlyList<Room> rooms = Region.Instance.Rooms;
        if (rooms == null)
        {
            return;
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i] == null)
            {
                continue;
            }
            rooms[i].ResetRoom();
        }
    }
}
