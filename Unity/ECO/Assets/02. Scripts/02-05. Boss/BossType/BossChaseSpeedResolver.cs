using UnityEngine;

public sealed class BossChaseSpeedResolver
{
    private readonly BossDataSO _bossData;
    private Transform _targetTransform;

    public BossChaseSpeedResolver(BossDataSO bossData)
    {
        _bossData = bossData;
    }

    public void SetTarget(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }

    public float Resolve(Vector2 bossPosition, EBossState bossState, float currentSpeed)
    {
        if (bossState == EBossState.Berserk)
        {
            return _bossData.CatchUpSpeed;
        }

        if (_targetTransform == null)
        {
            return _bossData.BaseSpeed;
        }

        Vector2 offset = (Vector2)_targetTransform.position - bossPosition;
        float sqrDistance = offset.sqrMagnitude;
        float catchUpStartDistance = _bossData.CatchUpStartDistance;
        float catchUpEndDistance = _bossData.CatchUpEndDistance;

        if (catchUpStartDistance * catchUpStartDistance <= sqrDistance)
        {
            return _bossData.CatchUpSpeed;
        }

        if (sqrDistance <= catchUpEndDistance * catchUpEndDistance)
        {
            return _bossData.BaseSpeed;
        }

        return currentSpeed;
    }
}
