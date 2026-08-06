using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    private readonly int _stateHash = Animator.StringToHash("State");

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void SetState(EBossState newState)
    {
        switch (newState)
        {
            case EBossState.Idle:
                _animator.SetInteger(_stateHash, 0);
                break;
            case EBossState.Chasing:
            case EBossState.Jumping:
            case EBossState.ReadyToJump:
                _animator.SetInteger(_stateHash, 1);
                break;
            case EBossState.Groggy:
                break;
            case EBossState.Berserk:
                _animator.SetInteger(_stateHash, 2);
                break;
        }

    }

    public void Pause()
    {
        _animator.speed = 0f;
    }

    public void Resume()
    {
        _animator.speed = 1f;
    }

}
