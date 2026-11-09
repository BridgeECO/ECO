using UnityEngine;

/// <summary>
/// 데미지 무적 시간을 재고, 무적인 동안 플레이어 스프라이트를 깜빡인다.
/// </summary>
public class PlayerInvincibility
{
    private const float DURATION_INVINCIBILITY = 2f;
    private const float SPEED_FLICKER = 8f;

    private readonly SpriteRenderer _spriteRenderer;
    private float _remainingTime;

    public bool IsInvincible { get; private set; }

    public PlayerInvincibility(SpriteRenderer spriteRenderer)
    {
        _spriteRenderer = spriteRenderer;
    }

    public void Begin()
    {
        IsInvincible = true;
        _remainingTime = DURATION_INVINCIBILITY;
    }

    /// <summary>
    /// 무적 시간을 줄이고 스프라이트를 깜빡인다. 이번 호출에서 무적이 끝났으면 true를 돌려준다.
    /// </summary>
    public bool Tick(float deltaTime)
    {
        if (!IsInvincible)
        {
            return false;
        }

        _remainingTime -= deltaTime;
        if (_remainingTime <= 0f)
        {
            Stop();
            return true;
        }
        SetSpriteAlpha(Mathf.PingPong(Time.time * SPEED_FLICKER, 1f));
        return false;
    }

    public void Stop()
    {
        IsInvincible = false;
        _remainingTime = 0f;
        SetSpriteAlpha(1f);
    }

    private void SetSpriteAlpha(float alpha)
    {
        Color color = _spriteRenderer.color;
        color.a = alpha;
        _spriteRenderer.color = color;
    }
}
