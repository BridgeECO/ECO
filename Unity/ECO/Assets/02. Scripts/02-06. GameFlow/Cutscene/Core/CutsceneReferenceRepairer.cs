#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>두 자리가 같은 스텝 객체를 나눠 갖고 있으면 떼어 놓는다.</summary>
public static class CutsceneReferenceRepairer
{
    // "이미 어딘가에서 봤다"를 재는 장부. 그룹 안팎에 걸친 중복도 잡으려면 순회 전체에 걸쳐 공유해야 한다.
    private static readonly HashSet<CutsceneStepBase> _seen = new HashSet<CutsceneStepBase>();

    // 복제 재귀가 지금 밟고 있는 경로. 순환을 만나면 여기서 걸린다. _seen과 의미가 달라 따로 둔다.
    private static readonly HashSet<CutsceneStepBase> _cloneStack = new HashSet<CutsceneStepBase>();

    /// <summary>
    /// 리스트의 '+'는 마지막 원소를 복제하는데 [SerializeReference]는 참조만 복사된다.
    /// 두면 한쪽 수치를 고칠 때 다른 쪽까지 바뀌고, 두 스텝이 재생 상태를 나눠 쓴다.
    /// </summary>
    public static bool Repair(CutsceneDirector director)
    {
        if (director == null)
        {
            return false;
        }

        _seen.Clear();
        return RepairList(director.EditableSteps, 1);
    }

    private static bool RepairList(List<CutsceneStepBase> steps, int depth)
    {
        if (steps == null || CutsceneGroupStepBase.MAX_NEST_DEPTH < depth)
        {
            return false;
        }

        bool isRepaired = false;

        for (int i = 0; i < steps.Count; i++)
        {
            CutsceneStepBase step = steps[i];
            if (ReferenceEquals(step, null))
            {
                continue;
            }

            // 처음 보는 객체면 그 자리가 원본이다. 두 번째부터가 복제로 생긴 중복이다.
            if (!_seen.Add(step))
            {
                _cloneStack.Clear();
                step = Clone(step, depth);
                steps[i] = step;
                isRepaired = true;

                if (ReferenceEquals(step, null))
                {
                    continue;
                }

                _seen.Add(step);
            }

            if (step is CutsceneGroupStepBase group && RepairList(group.Steps, depth + 1))
            {
                isRepaired = true;
            }
        }

        return isRepaired;
    }

    private static CutsceneStepBase Clone(CutsceneStepBase source, int depth)
    {
        // 순환이거나 너무 깊다. StackOverflowException은 catch가 불가능해 에디터가 그대로 죽는다.
        if (CutsceneGroupStepBase.MAX_NEST_DEPTH < depth || !_cloneStack.Add(source))
        {
            return null;
        }

        CutsceneStepBase clone = (CutsceneStepBase)Activator.CreateInstance(source.GetType());

        // JsonUtility는 [Serializable] 필드와 UnityEngine.Object 참조를 그대로 옮겨 준다.
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), clone);

        // 다만 자식은 오지 않는다. List<CutsceneStepBase>의 원소를 만들려면 추상 클래스를
        // 인스턴스화해야 하고, JsonUtility에는 어떤 파생 타입인지 적혀 있지도 않다.
        // 분기를 구체 타입이 아니라 추상 베이스로 두어 앞으로 늘어날 그룹 계열도 함께 걸린다.
        if (source is CutsceneGroupStepBase sourceGroup && clone is CutsceneGroupStepBase cloneGroup)
        {
            cloneGroup.Steps.Clear();
            for (int i = 0; i < sourceGroup.Steps.Count; i++)
            {
                CutsceneStepBase child = sourceGroup.Steps[i];
                cloneGroup.Steps.Add(ReferenceEquals(child, null) ? null : Clone(child, depth + 1));
            }
        }

        _cloneStack.Remove(source);
        return clone;
    }
}
#endif
