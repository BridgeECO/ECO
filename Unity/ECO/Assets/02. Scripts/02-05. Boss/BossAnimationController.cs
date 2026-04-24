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

    public void SetChasingState(bool isChasing)
    {
        
    }
}
