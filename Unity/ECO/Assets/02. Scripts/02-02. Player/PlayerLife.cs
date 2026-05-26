using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private const float DURATION_INVINCIBILITY = 2f;
    private const float SPEED_FLICKER = 8f;

    private float _invincibilityTimer;
    private bool _isInvincible;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isInvincible = false;
    }

    private void Start()
    {
        EventManager.Instance.AddEventListener(EEventType.PlayerDied, ResetInvincibility);
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

    private void OnDestroy()
    {
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, ResetInvincibility);
        }
    }

    public void RelayTriggerEnter2D(Collider2D other)
    {
        if (_isInvincible)
        {
            return;
        }

        if (other.CompareTag(nameof(ETags.InstantKill)))
        {
            LifeManager.Instance.InstantKill();
            return;
        }

        if (other.CompareTag(nameof(ETags.Obstacle)))
        {
            LifeManager.Instance.TakeDamage();
            BeginInvincibility();
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
        _isInvincible = true;
        _invincibilityTimer = DURATION_INVINCIBILITY;
    }

    private void StopInvincibility()
    {
        _isInvincible = false;
        _invincibilityTimer = 0f;
        Color color = _spriteRenderer.color;
        color.a = 1f;
        _spriteRenderer.color = color;
    }

    public void ResetInvincibility()
    {
        StopInvincibility();
    }
}
