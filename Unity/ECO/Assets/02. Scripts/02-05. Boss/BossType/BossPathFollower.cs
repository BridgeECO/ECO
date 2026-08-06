using System.Collections.Generic;
using UnityEngine;

public sealed class BossPathFollower
{
    private readonly Rigidbody2D _rigidbody;
    private IReadOnlyList<Vector3> _paths;
    private int _targetIndex;

    public bool HasPath => _paths is not null && 0 < _paths.Count;
    public Vector3 EndPoint => HasPath ? _paths[_paths.Count - 1] : _rigidbody.position;

    public BossPathFollower(Rigidbody2D rigidbody)
    {
        _rigidbody = rigidbody;
    }

    public void SetPath(IReadOnlyList<Vector3> paths)
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

        Vector2 targetPosition = _paths[_targetIndex];
        float remainingDistance = Vector2.Distance(currentPosition, targetPosition);
        float movementDistance = speed * Time.fixedDeltaTime;

        if (remainingDistance <= movementDistance)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.MovePosition(targetPosition);
            _targetIndex++;
            return;
        }

        Vector2 direction = (targetPosition - currentPosition).normalized;
        _rigidbody.linearVelocity = direction * speed;
    }
}
