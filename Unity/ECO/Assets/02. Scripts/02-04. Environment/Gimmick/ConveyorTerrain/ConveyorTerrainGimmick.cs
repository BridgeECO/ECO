using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ConveyorTerrainGimmick : TerrainGimmickBase, IGimmickPathVisualizable
{
    private TerrainGimmickEntry _entry;
    private CancellationTokenSource _conveyorCts;
    private bool _isInitialized;
    private int _currentIndex;
    private LineRenderer _pathLinePrefab;
    private GimmickPathVisualizer _pathVisualizer;
    private TerrainRiderSynchronizer _synchronizer;
    private bool _isPathVisible;

    public ConveyorTerrainGimmick(EGimmickActivationType activationType, bool isInverted, TerrainGimmickEntry entry, LineRenderer pathLinePrefab, bool isPathVisible)

        : base(activationType, isInverted)
    {
        _entry = entry;
        _pathLinePrefab = pathLinePrefab;
        _isPathVisible = isPathVisible;
    }

    public override void OnDestroy(TerrainObject target)
    {
        base.OnDestroy(target);
        HidePath();
        _conveyorCts?.Cancel();
        _conveyorCts?.Dispose();
    }

    public override void ResetGimmick(TerrainObject target)
    {
        base.ResetGimmick(target);
        CancelToken();
        _currentIndex = 1;

        if (_isInitialized)
        {
            if (target.Rigidbody != null)
            {
                target.Rigidbody.position = target.InitialPosition;
            }
            else
            {
                target.transform.position = target.InitialPosition;
            }
        }
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        EnsureComponents(target);
        RenewToken();

        if (isActivated)
        {
            StartConveyor(target);
        }
        else
        {
            StopConveyor();
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

    private void EnsureComponents(TerrainObject target)
    {
        EnsureRigidbody(target);
        EnsureSynchronizer(target);
        EnsureInitialized();
    }

    private void EnsureRigidbody(TerrainObject target)
    {
        if (target.Rigidbody != null)
        {
            return;
        }

        target.Rigidbody = target.gameObject.AddComponent<Rigidbody2D>();
        target.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        target.Rigidbody.useFullKinematicContacts = true;
    }

    private void EnsureSynchronizer(TerrainObject target)
    {
        if (_synchronizer != null)
        {
            return;
        }

        _synchronizer = target.GetComponent<TerrainRiderSynchronizer>();

        if (_synchronizer == null)
        {
            _synchronizer = target.gameObject.AddComponent<TerrainRiderSynchronizer>();
        }
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        if (!HasValidWaypoints())
        {
            return;
        }

        _currentIndex = 1;
        _isInitialized = true;
        _pathVisualizer = new GimmickPathVisualizer(_pathLinePrefab, GetStartPosition(), _entry.Waypoints);
    }

    private void CancelToken()
    {
        _conveyorCts?.Cancel();
        _conveyorCts?.Dispose();
        _conveyorCts = null;
    }

    private void RenewToken()
    {
        CancelToken();
        _conveyorCts = CancellationTokenSource.CreateLinkedTokenSource(OwnerDestroyToken);
    }

    private void StartConveyor(TerrainObject target)
    {
        if (!HasValidWaypoints())
        {
            Debug.LogWarning($"[ConveyorTerrainGimmick] {target.name}에 Waypoints가 최소 2개 이상 설정되어야 합니다.");
            return;
        }

        if (_isPathVisible)
        {
            ShowPath(target.transform);
        }

        ConveyorAsync(target, _conveyorCts.Token).Forget();
    }

    private void StopConveyor()
    {
        HidePath();
        _synchronizer?.SetVelocity(Vector2.zero);
    }

    private async UniTask ConveyorAsync(TerrainObject target, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (target == null || target.Rigidbody == null)
            {
                return;
            }

            Vector2 targetPos = GetCurrentTargetPosition();
            Vector2 currentPos = target.Rigidbody.position;

            if (HasReachedTarget(currentPos, targetPos))
            {
                AdvanceIndex(target, targetPos);
                continue;
            }

            MoveTowardsTarget(target, currentPos, targetPos);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        }
    }

    private bool HasValidWaypoints()
    {
        return _entry.Waypoints != null && 2 <= _entry.Waypoints.Count;
    }

    private Vector2 GetStartPosition()
    {
        return _entry.Waypoints[0].position;
    }

    private Vector2 GetCurrentTargetPosition()
    {
        return _entry.Waypoints[_currentIndex].position;
    }

    private bool HasReachedTarget(Vector2 currentPos, Vector2 targetPos)
    {
        return Vector2.Distance(currentPos, targetPos) <= 0.001f;
    }

    private void AdvanceIndex(TerrainObject target, Vector2 targetPos)
    {
        _synchronizer?.SetVelocity(Vector2.zero);
        target.Rigidbody.MovePosition(targetPos);
        _currentIndex++;

        if (_entry.Waypoints.Count<= _currentIndex )
        {
            TeleportToStart(target);
        }
    }

    private void TeleportToStart(TerrainObject target)
    {
        target.Rigidbody.position = GetStartPosition();
        _currentIndex = 1;
    }

    private void MoveTowardsTarget(TerrainObject target, Vector2 currentPos, Vector2 targetPos)
    {
        Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, _entry.MoveSpeed * Time.fixedDeltaTime);
        Vector2 velocity = (nextPos - currentPos) / Time.fixedDeltaTime;
        _synchronizer?.SetVelocity(velocity);
        target.Rigidbody.MovePosition(nextPos);
    }
}
