using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathMoveTerrainGimmick : TerrainGimmickBase, IGimmickPathVisualizable
{
    private TerrainGimmickEntry _entry;
    private EPathEndBehaviour _endBehaviour;
    private bool _isReturningOnDeactivate;
    private LineRenderer _pathLinePrefab;
    private bool _isPathVisible;

    private TerrainPathWalker _walker = new TerrainPathWalker();
    private TerrainPathMover _mover = new TerrainPathMover();
    private GimmickPathVisualizer _pathVisualizer;
    private CancellationTokenSource _walkCts;
    private bool _isInitialized;

    public PathMoveTerrainGimmick(EGimmickActivationType activationType, bool isInverted, TerrainGimmickEntry entry,
        EPathEndBehaviour endBehaviour, bool isReturningOnDeactivate, LineRenderer pathLinePrefab, bool isPathVisible)
        : base(activationType, isInverted)
    {
        _entry = entry;
        _endBehaviour = endBehaviour;
        _isReturningOnDeactivate = isReturningOnDeactivate;
        _pathLinePrefab = pathLinePrefab;
        _isPathVisible = isPathVisible;
    }

    public override void OnDestroy(TerrainObject target)
    {
        base.OnDestroy(target);
        HidePath();
        CancelToken();
    }

    public override void ResetGimmick(TerrainObject target)
    {
        base.ResetGimmick(target);
        CancelToken();
        _walker.ResetProgress();
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        _mover.EnsureComponents(target);
        EnsureInitialized(target);
        RenewToken();

        if (!HasValidWaypoints(target))
        {
            return;
        }

        if (_isReturningOnDeactivate)
        {
            _walker.SetForward(isActivated);
        }

        if (isActivated && _isPathVisible)
        {
            ShowPath(target.transform);
        }
        else
        {
            HidePath();
        }

        // 역주행 복귀 옵션이 켜져 있으면 비활성 상태에서도 최초 위치까지 되돌아가야 하므로 순회를 계속 돌린다.
        if (isActivated || _isReturningOnDeactivate)
        {
            WalkAsync(target, _walkCts.Token).Forget();
            return;
        }

        _mover.SetRiderVelocity(Vector2.zero);
    }

    public void ShowPath(Transform parent)
    {
        _pathVisualizer?.Show(parent);
    }

    public void HidePath()
    {
        _pathVisualizer?.Hide();
    }

    private void EnsureInitialized(TerrainObject target)
    {
        if (_isInitialized || _entry.Waypoints == null || _entry.Waypoints.Count == 0)
        {
            return;
        }

        _walker.InitPath(target.Rigidbody.position, _endBehaviour);
        _pathVisualizer = new GimmickPathVisualizer(_pathLinePrefab, _walker.GetPathStartPosition(_entry.Waypoints), _entry.Waypoints);
        _isInitialized = true;
    }

    private bool HasValidWaypoints(TerrainObject target)
    {
        int requiredCount = _walker.GetRequiredWaypointCount();
        if (_entry.Waypoints != null && requiredCount <= _entry.Waypoints.Count)
        {
            return true;
        }

        Debug.LogWarning($"[PathMoveTerrainGimmick] {target.name}에 Waypoints가 최소 {requiredCount}개 이상 설정되어야 합니다.");
        return false;
    }

    private void CancelToken()
    {
        _walkCts?.Cancel();
        _walkCts?.Dispose();
        _walkCts = null;
    }

    private void RenewToken()
    {
        CancelToken();
        _walkCts = CancellationTokenSource.CreateLinkedTokenSource(OwnerDestroyToken);
    }

    private async UniTask WalkAsync(TerrainObject target, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (target == null || target.Rigidbody == null)
            {
                return;
            }

            if (!_walker.TryGetTargetPosition(_entry.Waypoints, out Vector2 targetPosition))
            {
                return;
            }

            Vector2 currentPosition = target.Rigidbody.position;
            if (_mover.HasReached(currentPosition, targetPosition))
            {
                ArriveAtTarget(target, targetPosition);
                continue;
            }

            _mover.MoveTowards(target, currentPosition, targetPosition, _entry.MoveSpeed);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        }
    }

    private void ArriveAtTarget(TerrainObject target, Vector2 targetPosition)
    {
        _mover.Arrive(target, targetPosition);
        _walker.Advance(_entry.Waypoints);

        if (_walker.TryConsumeTeleport(_entry.Waypoints, out Vector2 teleportPosition))
        {
            _mover.Teleport(target, teleportPosition);
        }
    }
}
