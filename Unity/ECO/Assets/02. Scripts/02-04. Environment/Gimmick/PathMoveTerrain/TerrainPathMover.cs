using UnityEngine;

/// <summary>
/// 경로 이동 지형의 물리 실행부. 키네마틱 강체와 탑승자 동기화 컴포넌트를 보장하고,
/// 한 물리 스텝만큼의 이동과 탑승자 속도 전달을 담당한다.
/// </summary>
public class TerrainPathMover
{
    private const float ARRIVAL_THRESHOLD = 0.001f;

    private TerrainRiderSynchronizer _synchronizer;

    public void EnsureComponents(TerrainObject target)
    {
        EnsureRigidbody(target);
        EnsureSynchronizer(target);
    }

    public bool HasReached(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= ARRIVAL_THRESHOLD;
    }

    public void SetRiderVelocity(Vector2 velocity)
    {
        if (_synchronizer == null)
        {
            return;
        }
        _synchronizer.SetVelocity(velocity);
    }

    public void MoveTowards(TerrainObject target, Vector2 currentPosition, Vector2 targetPosition, float moveSpeed)
    {
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.fixedDeltaTime);
        SetRiderVelocity((nextPosition - currentPosition) / Time.fixedDeltaTime);
        target.Rigidbody.MovePosition(nextPosition);
    }

    public void Arrive(TerrainObject target, Vector2 targetPosition)
    {
        SetRiderVelocity(Vector2.zero);
        target.Rigidbody.MovePosition(targetPosition);
    }

    // 순간이동은 보간 없이 즉시 자리를 옮겨야 하므로 MovePosition이 아니라 position에 직접 대입한다.
    public void Teleport(TerrainObject target, Vector2 position)
    {
        target.Rigidbody.position = position;
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
}
