using UnityEngine;

public class LifeManager : MonoBehaviourSingleton<LifeManager>
{
    private const int MaxLife = 3;
    private const int RespawnLife = 2;

    private int _currentLife;

    public int CurrentLife => _currentLife;
    public int LifeMax => MaxLife;

    protected override void Awake()
    {
        base.Awake();
        _currentLife = MaxLife;
    }

    public void TakeDamage()
    {
        _currentLife -= 1;
        if (_currentLife <= 0)
        {
            _currentLife = 0;
            EventManager.Instance.BroadcastEvent(EEventType.PlayerDied);
        }
        else
        {
            EventManager.Instance.BroadcastEvent(EEventType.LifeChanged);
        }
    }

    public void InstantKill()
    {
        EventManager.Instance.BroadcastEvent(EEventType.PlayerDied);
    }

    public void RecoverOne()
    {
        if (MaxLife <= _currentLife)
        {
            return;
        }
        _currentLife += 1;
        EventManager.Instance.BroadcastEvent(EEventType.LifeChanged);
    }

    public void RecoverToRoomTransition()
    {
        if (RespawnLife <= _currentLife)
        {
            return;
        }
        _currentLife = RespawnLife;
        EventManager.Instance.BroadcastEvent(EEventType.LifeChanged);
    }

    public void SetLifeOnRespawn()
    {
        _currentLife = RespawnLife;
        EventManager.Instance.BroadcastEvent(EEventType.LifeChanged);
    }
}
