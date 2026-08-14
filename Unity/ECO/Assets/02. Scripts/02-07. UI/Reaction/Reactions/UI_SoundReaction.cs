using System;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// UI 효과음을 재생한다. 되돌릴 상태가 없어 물러날 때 하는 일이 없다.
///
/// 상태 트리거에 걸면 그 상태로 들어갈 때 한 번만 울린다. 유지되는 동안 반복되지 않는 이유는
/// 채널 주인이 바뀔 때만 재생이 호출되기 때문이다.
/// </summary>
[Serializable]
[Preserve]
public class UI_SoundReaction : UI_InstantReactionBase
{
    [SerializeField]
    private ESfxClip _clip = ESfxClip.SUI_Common_Button;

    [SerializeField]
    [Tooltip("같은 소리가 이 시간 안에 다시 요청되면 무시합니다.")]
    private float _minInterval = 0.04f;

    public override EUIReactionChannel Channel => EUIReactionChannel.Sound;

    protected override void Apply(UI_ReactionContext context, GameObject target)
    {
        if (!UI_ReactionSfxThrottle.TryConsume(_clip, _minInterval))
        {
            return;
        }

        if (SoundManager.Instance == null)
        {
            return;
        }

        SoundManager.Instance.PlayUiSfx(_clip);
    }

    protected override void Revert(UI_ReactionContext context, GameObject target)
    {
    }
}
