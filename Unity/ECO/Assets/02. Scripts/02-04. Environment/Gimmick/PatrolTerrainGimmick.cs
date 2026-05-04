using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PatrolTerrainGimmick : TerrainGimmickBase, IGimmickPathVisualizable
{
    private TerrainGimmickEntry _entry;
    private CancellationTokenSource _patrolCts;
    private Vector2 _initialPosition;
    private bool _isInitialized;
    private int _currentIndex = 0;
    private bool _isMovingForward = true;
    private LineRenderer _pathLinePrefab;
    private GimmickPathVisualizer _pathVisualizer;

    public PatrolTerrainGimmick(EGimmickActivationType activationType, bool isInverted, TerrainGimmickEntry entry, LineRenderer pathLinePrefab)

        : base(activationType, isInverted)
    {
        _entry = entry;
        _pathLinePrefab = pathLinePrefab;
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        if (target.Rigidbody == null)
        {
            target.Rigidbody = target.gameObject.AddComponent<Rigidbody2D>();
            target.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            target.Rigidbody.useFullKinematicContacts = true;
        }

        if (target.GetComponent<TerrainRiderSynchronizer>() == null)
        {
            target.gameObject.AddComponent<TerrainRiderSynchronizer>();
        }

        if (!_isInitialized)
        {
            _initialPosition = target.Rigidbody.position;
            _isInitialized = true;
            _pathVisualizer = new GimmickPathVisualizer(_pathLinePrefab, _initialPosition, _entry.Waypoints);
        }

        _patrolCts?.Cancel();
        _patrolCts?.Dispose();
        _patrolCts = new CancellationTokenSource();

        if (isActivated)
        {
            if (_entry.Waypoints == null || _entry.Waypoints.Count == 0)
            {
                Debug.LogWarning($"[PatrolTerrainGimmick] {target.name}에 Waypoints가 설정되지 않았습니다.");
                return;
            }
            ShowPath(target.transform);
            PatrolAsync(target, _patrolCts.Token).Forget();
        }
        else
        {
            HidePath();
            target.GetComponent<TerrainRiderSynchronizer>()?.SetVelocity(Vector2.zero);
        }
    }

    public void ShowPath(Transform parent)
    {
        _pathVisualizer?.Show(parent);
    }

    public void HidePath()
    {
        _pathVisualizer?.Hide();
    }

    private async UniTask PatrolAsync(TerrainObject target, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (!TryGetTargetPosition(out Vector2 targetPos))
            {
                break;
            }

            Vector2 currentPos = target.Rigidbody.position;
            
            if (Vector2.Distance(currentPos, targetPos) <= 0.001f)
            {
                UpdateNextWaypoint(target, targetPos);
                continue;
            }

            MoveTowardsTarget(target, currentPos, targetPos);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        }
    }

    private bool TryGetTargetPosition(out Vector2 targetPos)
    {
        targetPos = _initialPosition;
        if (0 <= _currentIndex)
        {
            if (_entry.Waypoints.Count <= _currentIndex) return true;
            Transform wp = _entry.Waypoints[_currentIndex];
            if (wp == null)
            {
                return false;
            }
            targetPos = wp.position;
        }
        return true;
    }

    private void UpdateNextWaypoint(TerrainObject target, Vector2 targetPos)
    {
        target.GetComponent<TerrainRiderSynchronizer>()?.SetVelocity(Vector2.zero);
        target.Rigidbody.MovePosition(targetPos);
        
        _currentIndex += _isMovingForward ? 1 : -1;
        
        if (_entry.Waypoints.Count <= _currentIndex)
        {
            _isMovingForward = false;
            _currentIndex = Mathf.Max(-1, _entry.Waypoints.Count - 2);
        }
        
        if (_currentIndex < -1)
        {
            _isMovingForward = true;
            _currentIndex = 0;
        }
    }

    private void MoveTowardsTarget(TerrainObject target, Vector2 currentPos, Vector2 targetPos)
    {
        Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, _entry.MoveSpeed * Time.fixedDeltaTime);
        Vector2 velocity = (nextPos - currentPos) / Time.fixedDeltaTime;
        target.GetComponent<TerrainRiderSynchronizer>()?.SetVelocity(velocity);
        target.Rigidbody.MovePosition(nextPos);
    }

    public override void OnDestroy(TerrainObject target)
    {
        base.OnDestroy(target);
        HidePath();
        _patrolCts?.Cancel();
        _patrolCts?.Dispose();
    }
}
