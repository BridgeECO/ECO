using System.Collections.Generic;
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

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isInvincible = false;
    }

    private void OnEnable()
    {
        EventManager.Instance.AddEventListener(EEventType.PlayerRespawned, OnPlayerRespawned);
    }

    // 플레이어 루트의 조립자(PlayerStateMachine.Awake)가 주입한다.
    public void InitSoundHandler(PlayerSoundHandler soundHandler)
    {
        _soundHandler = soundHandler;
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
        RemoveEventListeners();
    }

    private void RemoveEventListeners()
    {
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerRespawned, OnPlayerRespawned);
        }
    }

    public void RelayTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Obstacle)))
        {
            _activeObstacles.Add(other);
        }

        if (_isInvincible)
        {
            return;
        }

        if (other.CompareTag(nameof(ETags.InstantKill)))
        {
            _soundHandler?.PlayDeathSfx();
            LifeManager.Instance.InstantKill();
            return;
        }

        if (other.CompareTag(nameof(ETags.Obstacle)))
        {
            BeginInvincibility();
            LifeManager.Instance.TakeDamage();
        }
    }

    public void RelayTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Obstacle)))
        {
            _activeObstacles.Remove(other);
        }
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

    private void OnPlayerRespawned()
    {
        _activeObstacles.Clear();
        StopInvincibilityAsync().Forget();
    }

    private async UniTaskVoid StopInvincibilityAsync()
    {
        await UniTask.Yield();
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
            BeginInvincibility();
            LifeManager.Instance.TakeDamage();
        }
    }

    private void CleanActiveObstacles()
    {
        _activeObstacles.RemoveWhere(col => col == null || !col.gameObject.activeInHierarchy || !col.enabled);
    }
}
