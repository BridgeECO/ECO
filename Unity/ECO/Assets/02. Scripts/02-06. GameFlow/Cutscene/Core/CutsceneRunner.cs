using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>스텝 목록을 위에서 아래로 재생한다. 소유권 반납과 중단 사유는 세션이 쥔다.</summary>
public class CutsceneRunner
{
    private readonly CutsceneSkipWatcher _watcher;

    private int _index;

    /// <summary>아직 재생하지 않았거나 중간에 끊긴 스텝의 위치.</summary>
    public int CurrentIndex => _index;

    public CutsceneRunner(CutsceneSkipWatcher watcher)
    {
        _watcher = watcher;
    }

    public async UniTask RunAsync(IReadOnlyList<CutsceneStepBase> steps, CutsceneContext context,
        CancellationToken skipToken, CancellationToken abortToken)
    {
        if (steps == null)
        {
            return;
        }

        for (_index = 0; _index < steps.Count; _index++)
        {
            // DOTween을 쓰는 스텝은 취소돼도 예외 없이 정상 완료한다(UniTask의 기본 TweenCancelBehaviour.Kill).
            // 이 확인이 없으면 러너가 취소를 모른 채 다음 스텝을 실제로 재생한다.
            // 취소를 삼키는 보스 컷씬 어댑터 뒤에서도 같은 역할을 한다.
            abortToken.ThrowIfCancellationRequested();

            if (skipToken.IsCancellationRequested)
            {
                break;
            }

            CutsceneStepBase step = steps[_index];

            // 인스펙터에서 타입을 고르지 않은 빈 항목과 꺼 둔 항목은 건너뛴다.
            if (ReferenceEquals(step, null) || !step.IsEnabled)
            {
                continue;
            }

            if (!await PlayStepAsync(step, context, skipToken, abortToken))
            {
                break;
            }

            context.CameraLease.Reassert();
        }

        if (skipToken.IsCancellationRequested && !abortToken.IsCancellationRequested)
        {
            ApplyFinalStatesFrom(steps, _index, context);
        }
    }

    /// <summary>계속 진행해도 되면 true, 스킵으로 끊겼으면 false를 돌려준다.</summary>
    private async UniTask<bool> PlayStepAsync(CutsceneStepBase step, CutsceneContext context,
        CancellationToken skipToken, CancellationToken abortToken)
    {
        // 끊으면 망가지는 스텝은 스킵 토큰을 받지 않고, 도는 동안 ESC 감시도 멈춘다.
        _watcher.SetSuspended(!step.IsSkippable);
        CancellationToken stepToken = step.IsSkippable ? skipToken : abortToken;

        try
        {
            await step.PlayAsync(context, stepToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            abortToken.ThrowIfCancellationRequested();

            // 우리 토큰과 무관한 취소도 올라온다(카메라 패닝이 자기 destroy 토큰을 링크한다).
            // 스킵으로 끊긴 경우만 여기서 삼키고, 나머지는 위로 올려 세션이 리스를 정리하게 둔다.
            if (!skipToken.IsCancellationRequested)
            {
                throw;
            }

            return false;
        }
        finally
        {
            _watcher.SetSuspended(false);
        }
    }

    /// <summary>
    /// 남은 스텝의 최종 상태를 한 프레임에 찍는다. 스킵과 중단 복구가 같은 기계를 쓴다.
    /// </summary>
    public static void ApplyFinalStatesFrom(IReadOnlyList<CutsceneStepBase> steps, int startIndex,
        CutsceneContext context)
    {
        if (steps == null)
        {
            return;
        }

        for (int i = Mathf.Max(0, startIndex); i < steps.Count; i++)
        {
            CutsceneStepBase step = steps[i];

            // 재생 경로와 똑같이 거른다. 여기서 어긋나면 꺼 둔 스텝이 스킵할 때만 발동한다.
            if (ReferenceEquals(step, null) || !step.IsEnabled)
            {
                continue;
            }

            try
            {
                step.ApplyFinalState(context);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
