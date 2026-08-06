using UnityEngine;

public sealed class BossChaseSpeedResolver
{
    private readonly BossDataSO _bossData;
    private Transform _playerTransform;

    public BossChaseSpeedResolver(BossDataSO bossData)
    {
        _bossData = bossData;
    }

    public float Resolve(Vector2 bossPosition, EBossState bossState, float currentSpeed)
    {
        if (bossState == EBossState.Berserk)
        {
            return _bossData.CatchUpSpeed;
        }

        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag(nameof(ETags.Player));
            if (player == null)
            {
                return _bossData.BaseSpeed;
            }

            _playerTransform = player.transform;
        }

        float distance = Vector2.Distance(bossPosition, _playerTransform.position);
        if (_bossData.CatchUpStartDistance <= distance)
        {
            return _bossData.CatchUpSpeed;
        }

        if (distance <= _bossData.CatchUpEndDistance)
        {
            return _bossData.BaseSpeed;
        }

        return currentSpeed;
    }
}
