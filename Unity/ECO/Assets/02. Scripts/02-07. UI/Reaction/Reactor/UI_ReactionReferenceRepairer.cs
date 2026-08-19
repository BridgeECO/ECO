#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 두 자리가 같은 리액션 객체를 나눠 갖고 있으면 떼어 놓는다.
/// </summary>
public static class UI_ReactionReferenceRepairer
{
    private static readonly HashSet<UI_ReactionBase> _seen = new HashSet<UI_ReactionBase>();

    /// <summary>
    /// 리스트의 '+'는 마지막 원소를 복제하는데 [SerializeReference]는 참조만 복사된다.
    /// 두면 한쪽 수치를 고칠 때 다른 쪽까지 바뀌고, 두 트리거가 트윈 핸들 하나를 나눠 쓴다.
    /// </summary>
    public static bool Repair(UI_Reactor reactor)
    {
        if (reactor == null)
        {
            return false;
        }

        _seen.Clear();
        bool isRepaired = false;

        IReadOnlyList<UI_ReactionEntry> entries = reactor.Entries;
        for (int e = 0; e < entries.Count; e++)
        {
            List<UI_ReactionBase> reactions = entries[e].Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                UI_ReactionBase reaction = reactions[r];

                // 처음 보는 객체면 그 자리가 원본이다. 두 번째부터가 복제로 생긴 중복이다.
                if (reaction == null || _seen.Add(reaction))
                {
                    continue;
                }

                reactions[r] = Clone(reaction);
                isRepaired = true;
            }
        }

        return isRepaired;
    }

    // JsonUtility는 [Serializable] 필드와 UnityEngine.Object 참조를 그대로 옮겨 주므로
    // 기획자가 입력해 둔 수치와 대상 지정이 복제본에도 남는다.
    private static UI_ReactionBase Clone(UI_ReactionBase source)
    {
        UI_ReactionBase clone = (UI_ReactionBase)Activator.CreateInstance(source.GetType());
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), clone);
        return clone;
    }
}
#endif
