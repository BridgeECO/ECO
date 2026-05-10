using ECO;
using UnityEngine;
using VInspector;

public class BossChasingTrigger : MonoBehaviour
{
    [Foldout("Settings")]
    [SerializeField]
    private EBoss _targetBossType;
    [SerializeField]
    [Tooltip("이 트리거에 닿았을 때 보스가 추격을 시작할지, 멈출지 선택하세요.")]
    private EBossState _triggerAction = EBossState.Chasing;

    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered)
        {
            return;
        }

        if (other.CompareTag(nameof(ETags.Player)) && _triggerAction == EBossState.Chasing)
        {
            ExecuteAction();
            return;
        }

        if(other.CompareTag(nameof(ETags.Boss)) && _triggerAction == EBossState.Idle)
        {
            ExecuteAction();
        }
    }

    private void ExecuteAction()
    {
        BossBase targetBoss = BossManager.Instance.GetBoss(_targetBossType);
        if (targetBoss == null)
        {
            return;
        }

        _hasTriggered = true;

        if (_triggerAction == EBossState.Chasing)
        {
            targetBoss.StartChase();
        }
        else if (_triggerAction == EBossState.Idle)
        {
            targetBoss.StopChase();
        }
    }
}
