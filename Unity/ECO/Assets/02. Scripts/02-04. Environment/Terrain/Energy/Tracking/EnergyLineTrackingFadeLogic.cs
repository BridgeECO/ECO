using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// 트래킹 전용 페이드 아웃/인을 담당하는 순수 C# 로직 클래스.
// Image 참조를 생성자로 주입받아 DOTween으로 알파를 조작한다.
// MonoBehaviour 없이 동작하며 EnergyLineTracker(MB)가 소유한다.
public class EnergyLineTrackingFadeLogic
{
    private readonly Image _fadePanel;
    private readonly float _fadeOutDuration;
    private readonly float _fadeInDuration;

    public EnergyLineTrackingFadeLogic(Image fadePanel, float fadeOutDuration, float fadeInDuration)
    {
        _fadePanel = fadePanel;
        _fadeOutDuration = fadeOutDuration;
        _fadeInDuration = fadeInDuration;
        SetAlpha(0f);
    }

    public async UniTask FadeOutAsync(CancellationToken ct)
    {
        await _fadePanel
            .DOFade(1f, _fadeOutDuration)
            .SetEase(Ease.InQuad)
            .ToUniTask(TweenCancelBehaviour.Kill, ct);
    }

    public async UniTask FadeInAsync(CancellationToken ct)
    {
        await _fadePanel
            .DOFade(0f, _fadeInDuration)
            .SetEase(Ease.OutQuad)
            .ToUniTask(TweenCancelBehaviour.Kill, ct);
    }

    // 시퀀스 중단 등 비정상 종료 시 패널이 화면에 남지 않도록 즉시 초기화한다.
    public void Reset()
    {
        _fadePanel.DOKill();
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color c = _fadePanel.color;
        c.a = alpha;
        _fadePanel.color = c;
    }
}
