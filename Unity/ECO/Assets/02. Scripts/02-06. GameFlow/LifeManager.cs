using UnityEngine;

// 라이프 수치 연산만 담당하는 순수 C# 클래스.
// Unity 생명주기와 씬 오브젝트에 의존하지 않으므로 MonoBehaviour가 아닌 POCO 싱글턴으로 관리한다.
// 라이프 변화 통지는 EventManager 브로드캐스트를 유지한다. (보스 시스템이 PlayerDied를 구독)
public class LifeManager : POCOSingleton<LifeManager>
{
    private const int MAX_LIFE = 3;
    private const int RESPAWN_LIFE = 2;

    private int _currentLife = MAX_LIFE;

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

    public void SetLifeOnRespawn()
    {
        CurrentLife = RESPAWN_LIFE;
    }

    /// <summary>
    /// 세이브 포인트 통과와 플레이 세션 시작 시 라이프를 최대치로 되돌린다.
    /// 인스턴스가 플레이 세션 내내 유지되므로,
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

    // 도메인 리로드 비활성화 환경에서 플레이 세션 간 상태 잔류 방지
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitStaticState()
    {
        ResetInstance();
    }
}
