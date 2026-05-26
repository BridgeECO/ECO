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

    private void Start()
    {
        EventManager.Instance.AddEventListener(EEventType.PlayerRespawned, SetLifeOnRespawn);
    }

    private void OnDestroy()
    {
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerRespawned, SetLifeOnRespawn);
        }
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

    public void RecoverToRoomTransition()
    {
        if (RESPAWN_LIFE <= CurrentLife)
        {
            return;
        }
        CurrentLife = RESPAWN_LIFE;
    }

    private void SetLifeOnRespawn()
    {
        CurrentLife = RESPAWN_LIFE;
    }
}