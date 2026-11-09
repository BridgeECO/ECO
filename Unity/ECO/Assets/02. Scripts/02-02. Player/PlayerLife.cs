using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerLife : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private PlayerSoundHandler _soundHandler;
    private readonly HashSet<Collider2D> _activeObstacles = new();

    private const float DURATION_INVINCIBILITY = 2f;
    private const float SPEED_FLICKER = 8f;

    private float _invincibilityTimer;
    private bool _isInvincible;
    // PlayerDied부터 PlayerRespawned까지 참이다. 그사이에 들어온 즉사·피해는 이미 시작된 사망에 딸린 것이라 버린다.
    private bool _isDying;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isInvincible = false;
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
        if (!_isInvincible)
        {
            return;
        }

        _invincibilityTimer -= Time.deltaTime;
        if (_invincibilityTimer <= 0f)
        {
            StopInvincibility();
            return;
        }
        UpdateSpriteAlpha();
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

    public void RelayTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Obstacle)))
        {
            _activeObstacles.Add(other);
            TakeDamage();
            return;
        }

        if (other.CompareTag(nameof(ETags.InstantKill)))
        {
            InstantKill(EPlayerDeathCause.Fall);
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
    public void InstantKill(EPlayerDeathCause cause)
    {
        // 여러 콜라이더가 한 스텝에 함께 닿거나 리스폰 페이드 중에 다시 닿아도 사망은 한 번만 처리한다.
        if (_isDying)
        {
            return;
        }
        _isDying = true;
        _soundHandler?.PlayDeathSfx();

        if (cause == EPlayerDeathCause.Fall)
        {
            BroadcastFall();
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

    private void TakeDamage()
    {
        if (_isInvincible || _isDying)
        {
            return;
        }
        BeginInvincibility();
        LifeManager.Instance.TakeDamage();
    }

    private void UpdateSpriteAlpha()
    {
        float alpha = Mathf.PingPong(Time.time * SPEED_FLICKER, 1f);
        Color color = _spriteRenderer.color;
        color.a = alpha;
        _spriteRenderer.color = color;
    }

    private void BeginInvincibility()
    {
        _soundHandler?.PlayDamageSfx();
        _isInvincible = true;
        _invincibilityTimer = DURATION_INVINCIBILITY;
    }

    // 보스 접촉과 리스폰 버튼은 PlayerLife를 거치지 않고 사망시키므로 사망 표시를 여기서도 세운다.
    private void OnPlayerDied()
    {
        _isDying = true;
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
        StopInvincibility();
    }

    private void StopInvincibility()
    {
        _isInvincible = false;
        _invincibilityTimer = 0f;
        Color color = _spriteRenderer.color;
        color.a = 1f;
        _spriteRenderer.color = color;

        CleanActiveObstacles();
        if (_activeObstacles.Count > 0)
        {
            TakeDamage();
        }
    }

    private void CleanActiveObstacles()
    {
        _activeObstacles.RemoveWhere(col => col == null || !col.gameObject.activeInHierarchy || !col.enabled);
    }
}
