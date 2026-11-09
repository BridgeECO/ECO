using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VInspector;

public class PlayerLife : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Collider2D _bodyCollider;

    private PlayerSoundHandler _soundHandler;
    private PlayerInvincibility _invincibility;
    private PlayerHitEffectSpawner _hitEffectSpawner;
    private readonly HashSet<Collider2D> _activeObstacles = new();

    // PlayerDied부터 PlayerRespawned까지 참이다. 그사이에 들어온 즉사·피해는 이미 시작된 사망에 딸린 것이라 버린다.
    private bool _isDying;
    // 마지막 목숨을 잃는 피해면 LifeManager가 PlayerDied를 이 호출 안에서 곧바로 보낸다.
    private bool _isTakingDamage;

    private void Awake()
    {
        _invincibility = new PlayerInvincibility(GetComponent<SpriteRenderer>());
        _hitEffectSpawner = new PlayerHitEffectSpawner(_bodyCollider);
    }

    private void OnEnable()
    {
        if (EventManager.Instance == null)
        {
            return;
        }
        EventManager.Instance.AddEventListener(EEventType.PlayerDied, OnPlayerDied);
        EventManager.Instance.AddEventListener(EEventType.PlayerRespawned, OnPlayerRespawned);
    }

    private void Update()
    {
        if (_invincibility.Tick(Time.deltaTime))
        {
            TakeLingeringDamage();
        }
    }

    private void OnDisable()
    {
        _activeObstacles.Clear();
        _isDying = false;
        RemoveEventListeners();
    }

    private void RemoveEventListeners()
    {
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, OnPlayerDied);
            EventManager.Instance.RemoveEventListener(EEventType.PlayerRespawned, OnPlayerRespawned);
        }
    }

    // 플레이어 루트의 조립자(PlayerStateMachine.Awake)가 주입한다.
    public void InitSoundHandler(PlayerSoundHandler soundHandler)
    {
        _soundHandler = soundHandler;
    }

    public void RelayTriggerEnter2D(Collider2D playerCollider, Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Obstacle)))
        {
            _activeObstacles.Add(other);
            TakeDamage(playerCollider, other);
            return;
        }

        if (other.CompareTag(nameof(ETags.InstantKill)))
        {
            InstantKill(EPlayerDeathCause.Fall, playerCollider, other);
        }
    }

    public void RelayTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Obstacle)))
        {
            _activeObstacles.Remove(other);
        }
    }

    /// <summary>
    /// 즉사 판정. 남은 목숨과 데미지 무적에 관계없이 사망시킨다.
    /// </summary>
    /// <param name="playerCollider">닿은 플레이어 쪽 콜라이더</param>
    /// <param name="killer">닿은 상대 콜라이더. 둘 사이에 피격 이펙트를 띄운다.</param>
    public void InstantKill(EPlayerDeathCause cause, Collider2D playerCollider, Collider2D killer)
    {
        // 여러 콜라이더가 한 스텝에 함께 닿거나 리스폰 페이드 중에 다시 닿아도 사망은 한 번만 처리한다.
        if (_isDying)
        {
            return;
        }
        _isDying = true;
        _soundHandler?.PlayDeathSfx();

        // 기획서 목숨 차감 표에 낙사는 피격으로 올라 있지 않아 이펙트를 띄우지 않는다.
        if (cause == EPlayerDeathCause.Fall)
        {
            BroadcastFall();
        }
        else
        {
            _hitEffectSpawner.SpawnAtContact(playerCollider, killer);
        }

        LifeManager.Instance.InstantKill();
    }

    // 리스폰 연쇄(PlayerDied → RespawnAsync → ResetRoom)보다 먼저 알려야
    // 구독자가 리셋되기 전에 낙사를 집계할 수 있다.
    private void BroadcastFall()
    {
        if (EventManager.HasInstance)
        {
            EventManager.Instance.BroadcastEvent(EEventType.PlayerInstantKilled);
        }
    }

    private void TakeDamage(Collider2D playerCollider, Collider2D obstacle)
    {
        if (_invincibility.IsInvincible || _isDying)
        {
            return;
        }
        _hitEffectSpawner.SpawnAtContact(playerCollider, obstacle);
        _soundHandler?.PlayDamageSfx();
        _invincibility.Begin();

        _isTakingDamage = true;
        LifeManager.Instance.TakeDamage();
        _isTakingDamage = false;
    }

    // 보스 접촉과 리스폰 버튼은 PlayerLife를 거치지 않고 사망시키므로 사망 표시를 여기서도 세운다.
    private void OnPlayerDied()
    {
        // 즉사·피해로 여기서 시작한 사망이면 닿은 자리에 이펙트를 이미 띄웠다(낙사는 띄우지 않는다).
        bool isStartedHere = _isDying || _isTakingDamage;
        _isDying = true;
        if (isStartedHere)
        {
            return;
        }

        // 보스는 닿은 콜라이더를 알려 주지 않아 몸통과 겹친 보스 위치에 띄운다. 리스폰 버튼은 겹친 보스가 없어 아무것도 띄우지 않는다.
        _hitEffectSpawner.SpawnAtOverlappingBoss();
    }

    private void OnPlayerRespawned()
    {
        _isDying = false;
        _activeObstacles.Clear();
        StopInvincibilityAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid StopInvincibilityAsync(CancellationToken cancellationToken)
    {
        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        _invincibility.Stop();
        TakeLingeringDamage();
    }

    // 무적이 풀릴 때까지 장애물에 닿아 있었다면 그중 하나로 한 번 더 피해를 입는다.
    // 어느 플레이어 콜라이더가 닿아 있는지는 기억하지 않으므로 이펙트는 몸통 기준으로 띄운다.
    private void TakeLingeringDamage()
    {
        CleanActiveObstacles();
        foreach (Collider2D obstacle in _activeObstacles)
        {
            TakeDamage(_bodyCollider, obstacle);
            return;
        }
    }

    private void CleanActiveObstacles()
    {
        _activeObstacles.RemoveWhere(col => col == null || !col.gameObject.activeInHierarchy || !col.enabled);
    }
}
