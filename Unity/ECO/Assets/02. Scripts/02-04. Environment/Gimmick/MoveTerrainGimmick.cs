using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveTerrainGimmick : TerrainGimmickBase, IGimmickPathVisualizable
{
    private TerrainGimmickEntry _entry;
    private CancellationTokenSource _moveCts;
    private Vector2 _initialPosition;
    private bool _isInitialized;
    private int _targetWaypointIndex = 0;
    private bool _isCurrentlyForward = true;
    private LineRenderer _pathLinePrefab;
    private GimmickPathVisualizer _pathVisualizer;
    private TerrainRiderSynchronizer _synchronizer;

    public MoveTerrainGimmick(EGimmickActivationType activationType, bool isInverted, TerrainGimmickEntry entry, LineRenderer pathLinePrefab)

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

        if (_synchronizer == null)
        {
            _synchronizer = target.GetComponent<TerrainRiderSynchronizer>();
            if (_synchronizer == null)
            {
                _synchronizer = target.gameObject.AddComponent<TerrainRiderSynchronizer>();
            }
        }

        if (!_isInitialized)
        {
            _initialPosition = target.Rigidbody.position;
            _isInitialized = true;
            _targetWaypointIndex = 0;
            _isCurrentlyForward = true;
            _pathVisualizer = new GimmickPathVisualizer(_pathLinePrefab, _initialPosition, _entry.Waypoints);
        }

        _moveCts?.Cancel();
        _moveCts?.Dispose();
        _moveCts = new CancellationTokenSource();

        if (_entry.Waypoints == null || _entry.Waypoints.Count == 0)
        {
            Debug.LogWarning($"[MoveTerrainGimmick] {target.name}에 Waypoints가 설정되지 않았습니다.");
            return;
        }

        if (isActivated && !_isCurrentlyForward)
        {
            _isCurrentlyForward = true;
            _targetWaypointIndex++;
        }

        if (!isActivated && _isCurrentlyForward)
        {
            _isCurrentlyForward = false;
            _targetWaypointIndex--;
        }

        if (isActivated)
        {
            ShowPath(target.transform);
        }
        else
        {
            HidePath();
        }

        MoveRoutineAsync(target, isActivated, _moveCts.Token).Forget();
    }

    public void ShowPath(Transform parent)
    {
        _pathVisualizer?.Show(parent);
    }

    public void HidePath()
    {
        _pathVisualizer?.Hide();
    }

    private async UniTask MoveRoutineAsync(TerrainObject target, bool isForward, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_targetWaypointIndex < -1 || _entry.Waypoints.Count <= _targetWaypointIndex)
            {
                break;
            }

            Vector2 targetPos = _initialPosition;

            if (0 <= _targetWaypointIndex)
            {
                Transform wp = _entry.Waypoints[_targetWaypointIndex];
                if (wp == null)
                {
                    break;
                }
                
                targetPos = wp.position;
            }

            Vector2 currentPos = target.Rigidbody.position;
            
            if (Vector2.Distance(currentPos, targetPos) <= 0.001f)
            {
                _synchronizer?.SetVelocity(Vector2.zero);
                target.Rigidbody.MovePosition(targetPos);
                _targetWaypointIndex += isForward ? 1 : -1;
                continue;
            }

            Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, _entry.MoveSpeed * Time.fixedDeltaTime);
            Vector2 velocity = (nextPos - currentPos) / Time.fixedDeltaTime;
            _synchronizer?.SetVelocity(velocity);
            target.Rigidbody.MovePosition(nextPos);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        }
    }

    public override void OnDestroy(TerrainObject target)
    {
        base.OnDestroy(target);
        HidePath();
        _moveCts?.Cancel();
        _moveCts?.Dispose();
    }
}
