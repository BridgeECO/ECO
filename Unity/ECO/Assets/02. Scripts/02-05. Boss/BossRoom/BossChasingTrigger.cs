using ECO;
using UnityEngine;
using VInspector;

public class BossChasingTrigger : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private BossBase _targetBoss;

    [Foldout("Settings")]
    [SerializeField]
    [Tooltip("이 트리거에 닿았을 때 보스가 추격을 시작할지, 멈출지 선택하세요.")]
    private EBossState _triggerAction = EBossState.Chasing;

    private PlayerInput _playerInput;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Player)))
        {
            if (_targetBoss != null)
            {
                if (_triggerAction == EBossState.Chasing)
                {
                    _targetBoss.StartChase();
                }
                else if (_triggerAction == EBossState.Idle)
                {
                    _targetBoss.StopChase();
                    if (_playerInput = other.GetComponentInParent<PlayerInput>())
                    {
                        _playerInput.IsDashLocked = false;
                    }
                }
            }
        }
    }
}
