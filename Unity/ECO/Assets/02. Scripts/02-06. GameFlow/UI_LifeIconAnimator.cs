using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 목숨 아이콘 하나의 스프라이트 연출을 맡는다.
/// 남아 있는 목숨은 Idle을 반복하고, 잃을 때는 Damaged를 정방향으로, 되찾을 때는 역방향으로 한 번 돈다.
/// </summary>
public class UI_LifeIconAnimator
{
    private readonly Image _icon;
    private readonly IReadOnlyList<Sprite> _idleSprites;
    private readonly IReadOnlyList<Sprite> _damagedSprites;
    private readonly UI_SpriteFrameSettings _idleSettings;
    private readonly UI_SpriteFrameSettings _damagedSettings;
    private readonly UI_SpriteFrameRunner _runner = new UI_SpriteFrameRunner();

    // 명령마다 올리는 일련번호. 러너는 끊겨도 정상 종료처럼 풀려나므로, 뒤늦게 깨어난 이전 명령이
    // 다음 명령이 켜 둔 아이콘을 끄거나 그 위에 Idle을 덮어쓰지 못하게 이 값으로 걸러낸다.
    private int _generation;

    public UI_LifeIconAnimator(Image icon, IReadOnlyList<Sprite> idleSprites, float idleDuration,
        IReadOnlyList<Sprite> damagedSprites, float damagedDuration)
    {
        _icon = icon;
        _idleSprites = idleSprites;
        _damagedSprites = damagedSprites;
        _idleSettings = BuildSettings(idleSprites, idleDuration, true);
        _damagedSettings = BuildSettings(damagedSprites, damagedDuration, false);
    }

    public void Show(CancellationToken cancellationToken)
    {
        _generation++;
        _icon.enabled = true;
        PlayIdle(cancellationToken);
    }

    public void Hide()
    {
        Stop();
        _icon.enabled = false;
    }

    /// <summary>진행 중인 연출만 끊는다. 아이콘의 표시 여부는 건드리지 않는다.</summary>
    public void Stop()
    {
        _generation++;
        _runner.Stop();
    }

    public async UniTaskVoid PlayLoseAsync(CancellationToken cancellationToken)
    {
        int generation = ++_generation;
        _icon.enabled = true;

        await _runner.PlayAsync(_icon, _damagedSprites, _damagedSettings, true, cancellationToken);
        if (IsSuperseded(generation, cancellationToken))
        {
            return;
        }

        // 단발 재생은 마지막 장을 걸자마자 끝난다. 곧바로 끄면 마지막 장이 한 번도 그려지지 않아
        // 연출이 한 장만큼 짧아지므로, 한 장 길이만큼 더 보여 준 뒤 끈다.
        DelayType delayType = _damagedSettings.IsIgnoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_damagedSettings.FrameInterval), delayType,
            PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
        if (isCanceled || IsSuperseded(generation, cancellationToken))
        {
            return;
        }

        _icon.enabled = false;
    }

    public async UniTaskVoid PlayGainAsync(CancellationToken cancellationToken)
    {
        int generation = ++_generation;
        _icon.enabled = true;

        await _runner.PlayAsync(_icon, _damagedSprites, _damagedSettings, false, cancellationToken);
        if (IsSuperseded(generation, cancellationToken))
        {
            return;
        }

        PlayIdle(cancellationToken);
    }

    // 무한 반복이라 기다리지 않는다. 다음 명령이 러너를 다시 돌리면 러너가 이 재생을 끊는다.
    private void PlayIdle(CancellationToken cancellationToken)
    {
        _runner.PlayAsync(_icon, _idleSprites, _idleSettings, true, cancellationToken).Forget();
    }

    private bool IsSuperseded(int generation, CancellationToken cancellationToken)
    {
        return generation != _generation || cancellationToken.IsCancellationRequested;
    }

    // 기획 요구는 프레임 간격이 아니라 연출 전체 길이로 주어지므로, 장 수로 나눠 간격을 구한다.
    // 일시정지 메뉴가 timeScale을 0으로 만들어도 목숨 UI는 멈추지 않도록 unscaled로 돌린다.
    private static UI_SpriteFrameSettings BuildSettings(IReadOnlyList<Sprite> sprites, float duration, bool isLoop)
    {
        int frameCount = sprites is null ? 0 : sprites.Count;
        float frameInterval = 0 < frameCount ? duration / frameCount : duration;
        return new UI_SpriteFrameSettings(frameInterval, isLoop, duration, true);
    }
}
