using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 인스펙터 미리보기 버튼의 알맹이.
///
/// 에디트 모드는 지원하지 않는다. UniTask 플레이어루프도 DOTween도 물리도 돌지 않아
/// 절반만 동작하는 미리보기는 없느니만 못하다.
/// </summary>
public static class CutscenePreview
{
    /// <summary>1회성 판정을 무시하고 재생한다. 재진입 방지는 세션이 따로 한다.</summary>
    public static void Play(CutsceneDirector director, CutsceneRuntime runtime)
    {
        if (!IsReady())
        {
            return;
        }

        runtime.Progress.MarkFired();
        runtime.Session.PlayAsync(director, director.GetCancellationTokenOnDestroy()).Forget();
    }

    /// <summary>
    /// 모든 스텝의 최종 상태만 순서대로 찍는다.
    /// 이 프레임워크에서 가장 먼저 썩는 경로를 버튼 하나로 상시 노출시킨다.
    /// </summary>
    public static void ApplyFinalStates(IReadOnlyList<CutsceneStepBase> steps, CutsceneRuntime runtime)
    {
        if (!IsReady())
        {
            return;
        }

        runtime.Context.TryResolveCamera();
        CutsceneRunner.ApplyFinalStatesFrom(steps, 0, runtime.Context);
    }

    public static void Abort(CutsceneRuntime runtime)
    {
        if (!IsReady())
        {
            return;
        }

        runtime.Session.Abort(ECutsceneAbortReason.StopInPlace);
        runtime.Session.ForceRelease();
    }

    private static bool IsReady()
    {
        if (Application.isPlaying)
        {
            return true;
        }

        Debug.LogWarning("[CutsceneDirector] 미리보기는 플레이 중에만 동작합니다.");
        return false;
    }
}
