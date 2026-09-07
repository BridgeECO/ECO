using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>화면을 덮거나 걷는다. 컷씬 앞뒤에 한 쌍을 두면 스킵할 때 지형이 마저 움직이는 것을 가린다.</summary>
[Serializable]
[Preserve]
public class CutsceneFadeStep : CutsceneStepBase
{
    [SerializeField]
    [Tooltip("체크하면 화면을 덮고, 해제하면 걷습니다.")]
    private bool _isFadeOut = true;

    [SerializeField]
    [Min(0f)]
    private float _duration = 0.4f;

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        UIManager uiManager = UIManager.Instance;
        if (uiManager == null)
        {
            return;
        }

        if (_isFadeOut)
        {
            await uiManager.FadeOutAsync(_duration, cancellationToken);
            return;
        }

        await uiManager.FadeInAsync(_duration, cancellationToken);
    }

    /// <summary>
    /// FadeOutAsync는 await 뒤에 정리 코드가 없어, 취소되면 패널이 중간 알파로 활성인 채 남는다.
    /// 화면이 반쯤 덮인 상태로 게임이 이어지는 것을 막으려면 여기서 끝값을 찍어야 한다.
    /// </summary>
    public override void ApplyFinalState(CutsceneContext context)
    {
        UIManager uiManager = UIManager.Instance;
        if (uiManager == null)
        {
            return;
        }

        if (_isFadeOut)
        {
            uiManager.FadeOut(0f);
            return;
        }

        uiManager.FadeIn(0f);
    }
}
