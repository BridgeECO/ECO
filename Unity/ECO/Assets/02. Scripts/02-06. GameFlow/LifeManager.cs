using UnityEngine;

public class LifeManager : MonoBehaviourSingleton<LifeManager>
{
    private const int MAX_LIFE = 3;
    private const int RESPAWN_LIFE = 2;

    private int _currentLife;

    public int CurrentLife
    {
        get => _currentLife;
        private set
        {
            int clampedValue = Mathf.Clamp(value, 0, MAX_LIFE);
            if (_currentLife == clampedValue)
            {
                return;
            }
            _currentLife = clampedValue;
            EventManager.Instance.BroadcastEvent(EEventType.LifeChanged);
            if (_currentLife <= 0)
            {
                EventManager.Instance.BroadcastEvent(EEventType.PlayerDied);
            }
        }
    }
    public int LifeMax => MAX_LIFE;

    protected override void Awake()
    {
        base.Awake();
        _currentLife = MAX_LIFE;
    }

    public void SetLifeOnRespawn()
    {
        CurrentLife = RESPAWN_LIFE;
    }

    /// <summary>
    /// 세이브 포인트 통과와 플레이 세션 시작 시 라이프를 최대치로 되돌린다.
    /// 이 매니저는 DontDestroyOnLoad라 Awake가 앱 실행당 한 번만 돌기 때문에,
    /// 타이틀로 나갔다 새 게임을 시작하는 경로에서도 반드시 호출해야 한다.
    /// </summary>
    public void SetLifeToMax()
    {
        CurrentLife = MAX_LIFE;
    }

    public void TakeDamage()
    {
        CurrentLife -= 1;
    }

    public void InstantKill()
    {
        CurrentLife = 0;
    }

    public void Recover()
    {
        if (MAX_LIFE <= CurrentLife)
        {
            return;
        }
        CurrentLife += 1;
    }
}