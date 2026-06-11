using Cysharp.Threading.Tasks;
using UnityEngine;

public class BossFloorTransition : MonoBehaviour
{
    [SerializeField] 
    private EBoss _targetBossType;
    [SerializeField] 
    private Transform _jumpStartPoint;
    [SerializeField] 
    private Transform _jumpEndPoint;
    [SerializeField] 
    private float _jumpHeight = 10f;
    [SerializeField] 
    private float _horizontalForce = 5f;

    private BossBase _boss;
    private bool _hasTriggered = false;

    private void Start()
    {
        _boss = BossManager.Instance.GetBoss(_targetBossType);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered || !other.CompareTag(nameof(ETags.Player))) return;

        if (_boss is SilenceCityBoss silenceBoss)
        {
            GroggyEnd();
        }
    }

    public void GroggyEnd()
    {
        if (_hasTriggered) return;
        if (_boss is SilenceCityBoss silenceBoss)
        {
            _hasTriggered = true;
            Vector2 jumpVelocity = BossPhysicsUtility.CalculateParabolicVelocity(
                _jumpStartPoint.position,
                _jumpEndPoint.position,
                _jumpHeight,
                _horizontalForce
            );

            silenceBoss.FloorTransition(_jumpStartPoint, _jumpEndPoint, jumpVelocity).Forget();
        }
    }

    public void ResetFloorTransition()
    {
        _hasTriggered = false;
    }

    private void OnDrawGizmos()
    {
        if (_jumpStartPoint == null || _jumpEndPoint == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_jumpStartPoint.position, Vector3.one);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_jumpEndPoint.position, Vector3.one);

        Gizmos.color = Color.red;

        Vector2 vel = BossPhysicsUtility.CalculateParabolicVelocity(_jumpStartPoint.position, _jumpEndPoint.position, _jumpHeight, _horizontalForce);

        Gizmos.color = Color.red;
        int resolution = 30;
        float gravity = Mathf.Abs(Physics2D.gravity.y) * 50f;
        float totalTime = (vel.y / gravity) * 2;

        Vector2 lastPoint = _jumpStartPoint.position;
        for (int i = 1; i <= resolution; i++)
        {
            float t = (totalTime * i) / resolution;
            Vector2 nextPoint = (Vector2)_jumpStartPoint.position + BossPhysicsUtility.GetParabolicPosition(t, vel.x, vel.y);

            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}
