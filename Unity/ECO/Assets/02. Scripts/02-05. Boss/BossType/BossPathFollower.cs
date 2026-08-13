using System.Collections.Generic;
using UnityEngine;

public sealed class BossPathFollower
{
    private readonly Rigidbody2D _rigidbody;
    private IReadOnlyList<BossPathPoint> _paths;
    private int _targetIndex;

    public bool HasPath => _paths is not null && 0 < _paths.Count;
    public Vector3 EndPoint => HasPath ? _paths[_paths.Count - 1].Position : _rigidbody.position;

    public BossPathFollower(Rigidbody2D rigidbody)
    {
        _rigidbody = rigidbody;
    }

    public void SetPath(IReadOnlyList<BossPathPoint> paths)
    {
        _paths = paths;
        _targetIndex = 0;
    }

    public void Clear()
    {
        _paths = null;
        _targetIndex = 0;
        _rigidbody.linearVelocity = Vector2.zero;
    }

    public void Tick(Vector2 currentPosition, float speed)
    {
        if (!HasPath || _paths.Count <= _targetIndex)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        BossPathPoint targetPoint = _paths[_targetIndex];
        Vector2 targetPosition = targetPoint.Position;
        float adjustedSpeed = speed * targetPoint.SpeedMultiplier;
        float remainingDistance = Vector2.Distance(currentPosition, targetPosition);
        float movementDistance = adjustedSpeed * Time.fixedDeltaTime;

        if (remainingDistance <= movementDistance)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.MovePosition(targetPosition);
            _targetIndex++;
            return;
        }

        Vector2 direction = (targetPosition - currentPosition).normalized;
        _rigidbody.linearVelocity = direction * adjustedSpeed;
    }
}
