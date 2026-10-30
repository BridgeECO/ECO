using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 스냅포인트 안착 이동을 담당한다. 멈춘 지형을 경로를 따라 지정한 거리까지 옮기고, 순회 진행 상태를 그 위치에 맞춘다.
/// 진행 방향은 바꾸지 않으므로 다시 켜지면 안착한 좌표에서 원래 가던 방향으로 이어 간다.
/// 이동은 기존 순회와 같은 TerrainPathMover를 거치므로 탑승자 속도도 똑같이 전달된다.
/// </summary>
public class TerrainPathSettler
{
    private TerrainPathWalker _walker;
    private TerrainPathMover _mover;
    private TerrainPathMeasure _measure;

    // 소유 김믹이 순회를 다시 시작하거나 리셋하면 이 토큰이 취소되므로, 안착 상태도 그 즉시 풀린 것으로 본다.
    private CancellationToken _settleToken;
    private bool _isMoving;
    private bool _isArrived;

    public bool IsSettling => _isMoving && !_settleToken.IsCancellationRequested;
    public bool IsDocked => _isArrived && !_settleToken.IsCancellationRequested;

    public TerrainPathSettler(TerrainPathWalker walker, TerrainPathMover mover, TerrainPathMeasure measure)
    {
        _walker = walker;
        _mover = mover;
        _measure = measure;
    }

    // 순회가 향하던 구간(직전 꼭짓점 → 목표 꼭짓점) 위에 투영해, 경로가 겹치는 곳에서도 지금 지나는 자리의 거리를 얻는다.
    // 끝에 닿아 목표가 경로 밖으로 나간 Stop 경로는 끝 꼭짓점으로 본다.
    public float GetCurrentDistance(TerrainObject target)
    {
        int lastVertex = _measure.VertexCount - 1;
        int targetVertex = Mathf.Clamp(_walker.TargetVertex, 0, lastVertex);
        int previousVertex = _walker.IsMovingForward ? targetVertex - 1 : targetVertex + 1;
        if (previousVertex < 0 || lastVertex < previousVertex)
        {
            return _measure.GetVertexDistance(targetVertex);
        }
        return _measure.GetDistanceOnSegment(Mathf.Min(previousVertex, targetVertex), target.Rigidbody.position);
    }

    public async UniTask SettleAsync(TerrainObject target, float targetDistance, float speed, CancellationToken ct)
    {
        _settleToken = ct;
        _isArrived = false;
        _isMoving = true;
        try
        {
            await MoveAlongPathAsync(target, targetDistance, speed, ct);
            _isArrived = true;
        }
        finally
        {
            // 취소된 이전 안착의 마무리가 다음 물리 스텝에 늦게 돌아도, 새로 시작한 안착의 상태를 덮지 않게 한다.
            if (_settleToken == ct)
            {
                _isMoving = false;
            }
        }
    }

    // 꼭짓점을 넘을 때도 같은 속도를 유지하도록 거리 단위로 나아가고, 매 스텝 그 거리의 좌표로 이동한다.
    private async UniTask MoveAlongPathAsync(TerrainObject target, float targetDistance, float speed, CancellationToken ct)
    {
        float distance = GetCurrentDistance(target);

        // 멈춤 신호는 Update에서 오므로, 첫 이동부터 물리 스텝에서 하도록 한 스텝 기다린다.
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);

        while (distance != targetDistance)
        {
            distance = Mathf.MoveTowards(distance, targetDistance, speed * Time.fixedDeltaTime);
            _mover.MoveTowards(target, target.Rigidbody.position, _measure.GetPosition(distance), speed);
            SyncProgress(distance);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        }

        _mover.Arrive(target, _measure.GetPosition(targetDistance));
        SyncProgress(targetDistance);
    }

    // 안착 도중에 다시 켜져도 순회가 지금 구간의 다음 꼭짓점으로 이어 가도록 매 스텝 맞춘다.
    private void SyncProgress(float distance)
    {
        _walker.SetTargetVertex(_measure.FindNextVertex(distance, _walker.IsMovingForward));
    }
}
