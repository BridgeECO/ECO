using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스프라이트를 프레임 단위로 갈아 끼우는 재생 하나의 수명을 맡는다.
/// 컴포넌트(UI_SpriteAnimator)와 리액션(UI_SpriteSequenceReaction)이 이 한 벌을 공유한다.
/// </summary>
public class UI_SpriteFrameRunner
{
    private CancellationTokenSource _cts;

    // 재생마다 올려 두는 일련번호. 뒤늦게 풀려난 이전 재생이 지금 재생의 상태를 건드리지 못하게 막는다.
    private int _generation;

    public bool IsPlaying { get; private set; }

    /// <summary>
    /// 이전 재생을 끊고 처음부터 돌린다. 무한 반복은 끝나지 않으므로 호출부가 기다리면 영영 풀리지 않는다.
    /// </summary>
    public async UniTask PlayAsync(Image image, IReadOnlyList<Sprite> sprites,
        UI_SpriteFrameSettings settings, bool isForward, CancellationToken cancellationToken)
    {
        if (image == null || sprites == null || sprites.Count == 0)
        {
            return;
        }

        Stop();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        CancellationToken token = _cts.Token;

        // 앞선 재생은 다음 프레임에야 취소를 알아채고 finally로 들어온다. 그때는 이미 이 재생이
        // IsPlaying을 켜 둔 뒤라, 세대를 확인하지 않으면 남의 상태를 false로 덮어쓴다.
        int generation = ++_generation;
        IsPlaying = true;

        // 역재생은 되돌리는 동작이라 반복하지 않는다.
        bool isLoop = settings.IsLoop && isForward;

        try
        {
            do
            {
                float cycleStartTime = settings.GetTime();

                await PlayCycleAsync(image, sprites, settings, isForward, token);

                if (!isLoop)
                {
                    break;
                }

                await WaitNextCycleAsync(settings, cycleStartTime, token);
            }
            while (!token.IsCancellationRequested);
        }
        catch (OperationCanceledException)
        {
            // 취소는 이 시스템의 정상 경로다. 오브젝트가 꺼지거나 다음 재생이 들어올 때마다 지나간다.
        }
        finally
        {
            if (generation == _generation)
            {
                IsPlaying = false;
            }
        }
    }

    /// <summary>진행 중인 재생만 끊는다. 스프라이트 값은 건드리지 않으므로 복원은 호출부가 맡는다.</summary>
    public void Stop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        IsPlaying = false;
    }

    private static async UniTask PlayCycleAsync(Image image, IReadOnlyList<Sprite> sprites,
        UI_SpriteFrameSettings settings, bool isForward, CancellationToken token)
    {
        DelayType delayType = settings.IsIgnoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;

        for (int i = 0; i < sprites.Count; i++)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprites[isForward ? i : sprites.Count - 1 - i];

            // 마지막 프레임 뒤에 기다리면 한 회차가 프레임 하나만큼 길어진다.
            if (i == sprites.Count - 1)
            {
                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(settings.FrameInterval), delayType,
                PlayerLoopTiming.Update, token);
        }
    }

    // 회차 시작 시각을 기준으로 재므로, 재생에 걸린 시간이 간격보다 길면 곧바로 다음 회차로 넘어간다.
    private static async UniTask WaitNextCycleAsync(UI_SpriteFrameSettings settings, float cycleStartTime,
        CancellationToken token)
    {
        float remaining = settings.LoopInterval - (settings.GetTime() - cycleStartTime);

        // 간격이 이미 지났어도 한 프레임은 양보해야 반복문이 프레임을 통째로 잡아먹지 않는다.
        if (remaining <= 0f)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            return;
        }

        DelayType delayType = settings.IsIgnoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;
        await UniTask.Delay(TimeSpan.FromSeconds(remaining), delayType, PlayerLoopTiming.Update, token);
    }
}
