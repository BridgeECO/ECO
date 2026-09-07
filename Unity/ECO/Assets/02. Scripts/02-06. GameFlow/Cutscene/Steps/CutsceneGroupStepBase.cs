using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>자식 스텝 목록을 거느리는 스텝. 묶는 방식(동시/순차)은 파생이 정한다.</summary>
[Serializable]
public abstract class CutsceneGroupStepBase : CutsceneStepBase
{
    /// <summary>
    /// 중첩 상한.
    ///
    /// 유니티는 SerializeReference의 순환 참조를 파일에 그대로 저장하고 다시 읽는다.
    /// 그 상태로 재귀 순회를 돌면 StackOverflowException이 나는데 이 예외는 catch가 불가능해
    /// 에디터 프로세스가 즉사하고, 파일이 순환을 품고 있으므로 열 때마다 재현된다.
    /// 재귀를 도는 모든 자리가 이 값을 함께 지켜야 한다.
    /// </summary>
    public const int MAX_NEST_DEPTH = 6;

    [SerializeReference]
    private List<CutsceneStepBase> _steps = new List<CutsceneStepBase>();

    public List<CutsceneStepBase> Steps => _steps;

    public override void ApplyFinalState(CutsceneContext context)
    {
        ApplyFinalStates(_steps, context, 1);
    }

    /// <summary>
    /// 자식들의 최종 상태를 순서대로 찍는다. 하나가 던져도 나머지는 확정돼야 하므로 개별로 감싼다.
    /// </summary>
    public static void ApplyFinalStates(IReadOnlyList<CutsceneStepBase> steps, CutsceneContext context, int depth)
    {
        if (steps == null || MAX_NEST_DEPTH <= depth)
        {
            return;
        }

        for (int i = 0; i < steps.Count; i++)
        {
            CutsceneStepBase step = steps[i];

            // 인스펙터에서 타입을 고르지 않은 빈 항목과 꺼 둔 항목은 재생 경로와 똑같이 건너뛴다.
            // 여기서 어긋나면 "재생했을 때"와 "건너뛰었을 때"의 결과가 갈린다.
            if (ReferenceEquals(step, null) || !step.IsEnabled)
            {
                continue;
            }

            try
            {
                if (step is CutsceneGroupStepBase group)
                {
                    ApplyFinalStates(group.Steps, context, depth + 1);
                    continue;
                }

                step.ApplyFinalState(context);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
