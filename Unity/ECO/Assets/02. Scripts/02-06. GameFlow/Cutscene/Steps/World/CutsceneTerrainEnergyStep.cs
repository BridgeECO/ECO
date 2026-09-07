using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 지형에 에너지를 넣거나 끊는다. 지시만 하고 곧바로 반환한다.
///
/// 실제로 멈출 때까지 기다리는 것은 CutsceneWaitTerrainSettledStep이 따로 맡는다.
/// 상태를 바꾸는 스텝과 결과를 기다리는 스텝을 나눠야, 건너뛰었을 때
/// "기다림만 사라지고 상태는 전부 확정된다"가 구조적으로 보장된다.
/// </summary>
[Serializable]
[Preserve]
public class CutsceneTerrainEnergyStep : CutsceneStepBase
{
    [SerializeField]
    private List<TerrainObject> _targets = new List<TerrainObject>();

    [SerializeField]
    [Tooltip("MoveTerrain 기믹은 켜면 웨이포인트로, 끄면 초기 위치로 돌아갑니다.")]
    private bool _isEnergyActive = true;

    public override UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        Apply();
        return UniTask.CompletedTask;
    }

    public override void ApplyFinalState(CutsceneContext context)
    {
        Apply();
    }

    private void Apply()
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            TerrainObject target = _targets[i];
            if (target != null)
            {
                target.SetEnergyActive(_isEnergyActive);
            }
        }
    }
}
