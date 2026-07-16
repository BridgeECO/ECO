using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

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

                break;
            case EBossState.Chasing:

                break;
            case EBossState.Groggy:

                break;
            case EBossState.Berserk:

                break;
        }

    }
}
