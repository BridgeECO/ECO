using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 지형이 실제로 멈출 때까지 기다린다. 대기 시간을 수치로 맞추는 것을 대신하므로
/// 기획자가 기믹 속도를 바꿔도 타이밍이 어긋나지 않는다.
///
/// 속도가 아니라 위치 변화를 본다. MoveTerrainGimmick은 Rigidbody2D.MovePosition으로 움직여
/// linearVelocity가 0으로 남기 때문이다.
/// </summary>
[Serializable]
[Preserve]
public class CutsceneWaitTerrainSettledStep : CutsceneStepBase
{
    [SerializeField]
    private List<TerrainObject> _targets = new List<TerrainObject>();

    [SerializeField]
    [Min(0f)]
    [Tooltip("이 거리보다 적게 움직이면 멈춘 것으로 봅니다.")]
    private float _threshold = 0.02f;

    [SerializeField]
    [Min(1)]
    [Tooltip("이만큼 연속으로 멈춰 있어야 통과합니다. 방향을 바꾸는 순간을 멈춤으로 오인하지 않게 합니다.")]
    private int _settleFrames = 3;

    [SerializeField]
    [Min(0f)]
    private float _timeout = 6f;

    private List<Vector3> _lastPositions;

    private List<Vector3> LastPositions
    {
        get
        {
            if (_lastPositions == null)
            {
                _lastPositions = new List<Vector3>();
            }

            return _lastPositions;
        }
    }

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (_targets.Count == 0)
        {
            return;
        }

        List<Vector3> lastPositions = LastPositions;
        lastPositions.Clear();
        for (int i = 0; i < _targets.Count; i++)
        {
            lastPositions.Add(_targets[i] == null ? Vector3.zero : _targets[i].transform.position);
        }

        float sqrThreshold = _threshold * _threshold;
        float remaining = _timeout;
        int settledFrames = 0;

        while (settledFrames < _settleFrames && 0f < remaining)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
            remaining -= Time.fixedDeltaTime;
            settledFrames = IsSettled(lastPositions, sqrThreshold) ? settledFrames + 1 : 0;
        }
    }

    // 지형은 스스로 멈춘다. 기다림만 사라질 뿐 상태는 이미 확정돼 있다.
    public override void ApplyFinalState(CutsceneContext context)
    {
    }

    private bool IsSettled(List<Vector3> lastPositions, float sqrThreshold)
    {
        bool isSettled = true;

        for (int i = 0; i < _targets.Count; i++)
        {
            TerrainObject target = _targets[i];
            if (target == null)
            {
                continue;
            }

            Vector3 current = target.transform.position;
            if (sqrThreshold < (current - lastPositions[i]).sqrMagnitude)
            {
                isSettled = false;
            }

            lastPositions[i] = current;
        }

        return isSettled;
    }
}
