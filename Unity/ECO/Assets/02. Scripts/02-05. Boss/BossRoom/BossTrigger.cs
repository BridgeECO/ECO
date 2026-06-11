using Cysharp.Threading.Tasks;
using ECO;
using UnityEngine;
using VInspector;

public class BossTrigger : MonoBehaviour
{
    public enum ETriggerTarget { Player, Boss }

    [Foldout("Trigger Settings")]
    [SerializeField, Tooltip("트리거를 발동시킬 대상을 선택하세요.")]
    private ETriggerTarget _targetType = ETriggerTarget.Player;
    [SerializeField]
    private EBoss _targetBossType;
    [SerializeField]
    [Tooltip("이 트리거에 닿았을 때 변경할 보스 상태를 선택하세요.")]
    private EBossState _triggerAction = EBossState.Chasing;

    [Foldout("Cinematic")]
    [SerializeField]
    [Tooltip("실행할 연출 오브젝트를 넣으세요.")]
    private BossCinematicBase _cinematicSequence;

    [Foldout("Energy")]
    [SerializeField]
    [Tooltip("활성화할 에너지 오브젝트를 넣으세요.")]
    private BossEnergySwitch _energySwitch;

    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered)
        {
            return;
        }

        if (_targetType == ETriggerTarget.Player && other.CompareTag(nameof(ETags.Player)))
        {
            ExecuteAction();
        }
        else if (_targetType == ETriggerTarget.Boss && other.CompareTag(nameof(ETags.Boss)))
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

        StartCinematic(targetBoss).Forget();
    }

    private async UniTask StartCinematic(BossBase boss)
    {
        if (_cinematicSequence != null)
        {
            await _cinematicSequence.PlayCinematicAsync(boss);
        }
        else
        {
            if (_triggerAction == EBossState.Chasing)
            {
                boss.StartChase();
            }
            else if (_triggerAction == EBossState.Groggy)
            {
                boss.StartGroggy();
            }
            else if (_triggerAction == EBossState.Idle)
            {
                boss.StopChase();
            }
        }
    }
}
