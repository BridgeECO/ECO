using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VInspector;

public class BossFloorTransition : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    [Tooltip("이 지형 전환을 사용할 보스입니다. 새 보스전에서는 직접 할당하세요.")]
    private BossChasingCharacter _boss;

    [HideInInspector]
    [SerializeField] 
    private EBoss _targetBossType;
    [SerializeField] 
    private Transform _jumpStartPoint;
    [SerializeField] 
    private Transform _jumpEndPoint;
    [SerializeField]
    private float _jumpHeight;

    private bool _hasTriggered = false;

    private void Start()
    {
        if (_boss == null)
        {
            _boss = BossManager.Instance.GetBoss(_targetBossType) as BossChasingCharacter;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered || !other.CompareTag(nameof(ETags.Player)))
        {
            return;
        }

        StartTransition();
    }

    public void StartTransition()
    {
        if (_hasTriggered)
        {
            return;
        }

        if (_boss == null)
        {
            Debug.LogError($"[{nameof(BossFloorTransition)}] Boss reference is missing.", this);
            return;
        }

        if (_jumpStartPoint == null || _jumpEndPoint == null)
        {
            Debug.LogError($"[{nameof(BossFloorTransition)}] Jump points are missing.", this);
            return;
        }

        _hasTriggered = true;
        CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
        _boss.TransitionFloorAsync(
            _jumpStartPoint,
            _jumpEndPoint,
            _jumpHeight,
            cancellationToken).Forget();
    }
    public void ResetFloorTransition()
    {
        _hasTriggered = false;
    }

    private void OnDrawGizmos()
    {
        if (_jumpStartPoint == null || _jumpEndPoint == null)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_jumpStartPoint.position, Vector3.one);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_jumpEndPoint.position, Vector3.one);

        Gizmos.color = Color.red;
        int resolution = 30;

        Vector2 lastPoint = _jumpStartPoint.position;
        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector2 nextPoint = BossPhysicsUtility.GetGeometricParabola(
                _jumpStartPoint.position,
                _jumpEndPoint.position,
                _jumpHeight,
                t
            );

            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}
