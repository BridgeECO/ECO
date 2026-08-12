using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public sealed class BossTransitionMover
{
    private readonly Rigidbody2D _rigidbody;
    private readonly Transform _transform;
    private CancellationTokenSource _actionCts;

    public BossTransitionMover(Rigidbody2D rigidbody, Transform transform)
    {
        _rigidbody = rigidbody;
        _transform = transform;
    }

    public async UniTask MoveToPointAsync(Vector3 targetPosition, float speed, CancellationToken cancellationToken)
    {
        SetActionToken(cancellationToken);
        float targetX = targetPosition.x;
        float directionX = Mathf.Sign(targetX - _transform.position.x);

        while (Mathf.Sign(targetX - _transform.position.x) == directionX &&
               0.1f < Mathf.Abs(targetX - _transform.position.x))
        {
            _rigidbody.linearVelocity = new Vector2(directionX * speed, _rigidbody.linearVelocity.y);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, _actionCts.Token);
        }

        _rigidbody.position = new Vector2(targetX, _rigidbody.position.y);
        _rigidbody.linearVelocity = Vector2.zero;
    }

    public async UniTask JumpAsync(
        Vector2 startPosition,
        Vector2 endPosition,
        float jumpHeight,
        float jumpSpeed,
        CancellationToken cancellationToken)
    {
        SetActionToken(cancellationToken);
        float arcLength = BossPhysicsUtility.ApproximateParabolaLength(startPosition, endPosition, jumpHeight);
        float duration = arcLength / Mathf.Max(jumpSpeed, Mathf.Epsilon);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float interpolation = Mathf.Clamp01(elapsed / duration);
            Vector2 position = BossPhysicsUtility.GetGeometricParabola(
                startPosition,
                endPosition,
                jumpHeight,
                interpolation);
            _rigidbody.MovePosition(position);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, _actionCts.Token);
        }

        _rigidbody.MovePosition(endPosition);
        _rigidbody.linearVelocity = Vector2.zero;
    }

    public void Cancel()
    {
        if (_actionCts is null)
        {
            return;
        }

        _actionCts.Cancel();
        _actionCts.Dispose();
        _actionCts = null;
    }

    private void SetActionToken(CancellationToken cancellationToken)
    {
        Cancel();
        _actionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }
}
