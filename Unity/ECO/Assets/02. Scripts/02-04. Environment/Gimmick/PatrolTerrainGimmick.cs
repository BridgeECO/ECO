using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PatrolTerrainGimmick : TerrainGimmickBase
{
    private TerrainGimmickEntry _entry;
    private CancellationTokenSource _patrolCts;
    private Vector2 _initialPosition;
    private bool _isInitialized;

    public PatrolTerrainGimmick(EGimmickActivationType activationType, bool isInverted, TerrainGimmickEntry entry)

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
            PatrolAsync(target, _patrolCts.Token).Forget();
        }
        else
        {
            ReturnToInitialPositionAsync(target, _patrolCts.Token).Forget();
        }
    }

    private async UniTask PatrolAsync(TerrainObject target, CancellationToken ct)
    {
        int currentIndex = 0;
        int direction = 1;

        while (!ct.IsCancellationRequested)
        {
            Transform wp = _entry.Waypoints[currentIndex];
            if (wp == null)
            {
                break;
            }

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

            currentIndex += direction;
            if (_entry.Waypoints.Count <= currentIndex)
            {
                direction = -1;
                currentIndex = _entry.Waypoints.Count - 2;
                currentIndex = (currentIndex < 0) ? 0 : currentIndex;
            }
            else if (currentIndex < 0)
            {
                direction = 1;
                currentIndex = 1;
                currentIndex = (_entry.Waypoints.Count <= currentIndex) ? 0 : currentIndex;
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
        _patrolCts?.Cancel();
        _patrolCts?.Dispose();
    }
}
