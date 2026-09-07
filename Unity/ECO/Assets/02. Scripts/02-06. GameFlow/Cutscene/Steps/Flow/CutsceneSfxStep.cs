using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>효과음을 한 번 재생한다.</summary>
[Serializable]
[Preserve]
public class CutsceneSfxStep : CutsceneStepBase
{
    [SerializeField]
    private ESfxClip _clip;

    [SerializeField]
    [Tooltip("소리가 날 위치입니다. 비워 두면 컷씬 오브젝트 위치에서 납니다.")]
    private Transform _source;

    public override UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (!SoundManager.HasInstance)
        {
            return UniTask.CompletedTask;
        }

        Transform source = _source != null ? _source : context.Owner;
        SoundManager.Instance.PlayWorldSfx(_clip, source);
        return UniTask.CompletedTask;
    }

    // 건너뛴 소리를 몰아서 트는 것이 안 트는 것보다 나쁘다.
    public override void ApplyFinalState(CutsceneContext context)
    {
    }
}
