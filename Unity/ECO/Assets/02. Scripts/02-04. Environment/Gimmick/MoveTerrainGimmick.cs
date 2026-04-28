using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveTerrainGimmick : TerrainGimmickBase
{
    private TerrainGimmickEntry _entry;
    private CancellationTokenSource _moveCts;
    private Vector2 _initialPosition;
    private bool _isInitialized;

    public MoveTerrainGimmick(EGimmickActivationType activationType, bool isInverted, TerrainGimmickEntry entry)

        : base(activationType, isInverted)
    {
        _entry = entry;
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
        }

        _moveCts?.Cancel();
        _moveCts?.Dispose();
        _moveCts = new CancellationTokenSource();

        if (isActivated)
        {
            if (_entry.Waypoints == null || _entry.Waypoints.Count == 0)
            {
                Debug.LogWarning($"[MoveTerrainGimmick] {target.name}에 Waypoints가 설정되지 않았습니다.");
                return;
            }
            MoveWaypointsAsync(target, _moveCts.Token).Forget();
        }
        else
        {
            ReturnToInitialPositionAsync(target, _moveCts.Token).Forget();
        }
    }

    private async UniTask MoveWaypointsAsync(TerrainObject target, CancellationToken ct)
    {
        for (int i = 0; i < _entry.Waypoints.Count; i++)
        {
            if (ct.IsCancellationRequested) return;

            Transform wp = _entry.Waypoints[i];
            if (wp == null) continue;

            Vector2 targetPos = wp.position;

            while (!ct.IsCancellationRequested)
            {
                Vector2 currentPos = target.Rigidbody.position;
                if (Vector2.Distance(currentPos, targetPos) <= 0.001f)
                {
                    target.Rigidbody.MovePosition(targetPos);
                    break;
                }

                Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, _entry.MoveSpeed * Time.fixedDeltaTime);
                target.Rigidbody.MovePosition(nextPos);

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
            }
        }
    }

    private async UniTask ReturnToInitialPositionAsync(TerrainObject target, CancellationToken ct)
    {
        Vector2 targetPos = _initialPosition;

        while (!ct.IsCancellationRequested)
        {
            Vector2 currentPos = target.Rigidbody.position;
            if (Vector2.Distance(currentPos, targetPos) <= 0.001f)
            {
                target.Rigidbody.MovePosition(targetPos);
                break;
            }

            Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, _entry.MoveSpeed * Time.fixedDeltaTime);
            target.Rigidbody.MovePosition(nextPos);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        }
    }

    public override void OnDestroy(TerrainObject target)
    {
        base.OnDestroy(target);
        _moveCts?.Cancel();
        _moveCts?.Dispose();
    }
}
