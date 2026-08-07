using System;
using System.Threading;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 대화 텍스트 타이핑·페이지 표시를 담당한다. Unity 생명주기가 필요 없어
/// 컴포넌트가 아닌 직렬화 클래스로 두고, UI_NPCDialogue가 인라인 필드로 소유한다.
/// </summary>
[Serializable]
public class UI_NPCDialogueTextBox
{
    [SerializeField]
    private TextMeshProUGUI _dialogueText;

    [SerializeField]
    private TextMeshProUGUI _pageIndicatorText;

    [SerializeField]
    private GameObject _continueIndicator;

    [SerializeField]
    private float _fadeDuration;

    [SerializeField]
    private float _textPrintSpeed;

    private Tween _typewriterTween;

    public async UniTask ShowPageAsync(string text, int currentIndex, int totalCount, CancellationToken cancellationToken)
    {
        SetText(text);
        RefreshPageIndicator(currentIndex, totalCount);
        RefreshContinueIndicator(currentIndex, totalCount);
        SetTextAlpha(1f);
        await PlayTypewriterAsync(cancellationToken);
    }

    public async UniTask HideAsync(CancellationToken cancellationToken)
    {
        KillTypewriter();
        await PlayTextFadeOutAsync(cancellationToken);
    }

    public void SkipPrinting()
    {
        if (_typewriterTween is not null && _typewriterTween.IsActive())
        {
            _typewriterTween.Complete();
        }
    }

    private void SetText(string text)
    {
        if (_dialogueText == null)
        {
            return;
        }

        _dialogueText.text = text;
    }

    private void RefreshPageIndicator(int currentIndex, int totalCount)
    {
        if (_pageIndicatorText == null)
        {
            return;
        }

        if (totalCount <= 1)
        {
            _pageIndicatorText.gameObject.SetActive(false);
            return;
        }

        _pageIndicatorText.gameObject.SetActive(true);
        _pageIndicatorText.text = $"{currentIndex + 1} / {totalCount}";
    }

    private void RefreshContinueIndicator(int currentIndex, int totalCount)
    {
        if (_continueIndicator == null)
        {
            return;
        }

        bool isLastPage = totalCount - 1<= currentIndex ;
        _continueIndicator.SetActive(!isLastPage);
    }

    private void SetTextAlpha(float alpha)
    {
        if (_dialogueText == null)
        {
            return;
        }

        Color color = _dialogueText.color;
        color.a = alpha;
        _dialogueText.color = color;
    }

    private void KillTypewriter()
    {
        if (_typewriterTween is not null && _typewriterTween.IsActive())
        {
            _typewriterTween.Kill();
        }
        _typewriterTween = null;
    }

    private async UniTask PlayTypewriterAsync(CancellationToken cancellationToken)
    {
        if (_dialogueText == null)
        {
            return;
        }

        KillTypewriter();

        _dialogueText.maxVisibleCharacters = 0;
        _dialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = _dialogueText.textInfo.characterCount;

        float duration = 0f < _textPrintSpeed ? totalVisibleCharacters / _textPrintSpeed : 0f;

        if (duration <= 0f)
        {
            _dialogueText.maxVisibleCharacters = totalVisibleCharacters;
            return;
        }

        _typewriterTween = DOTween.To(
            () => _dialogueText.maxVisibleCharacters,
            x => _dialogueText.maxVisibleCharacters = x,
            totalVisibleCharacters,
            duration
        )
        .SetEase(Ease.Linear)
        .SetUpdate(UpdateType.Normal, true);

        await _typewriterTween.ToUniTask(cancellationToken: cancellationToken);
        if (_dialogueText == null)
        {
            return;
        }

        _dialogueText.maxVisibleCharacters = totalVisibleCharacters;
    }

    private async UniTask PlayTextFadeOutAsync(CancellationToken cancellationToken)
    {
        if (_dialogueText == null)
        {
            return;
        }

        _dialogueText.DOKill();
        await _dialogueText.DOFade(0f, _fadeDuration).SetEase(Ease.InQuad)
            .ToUniTask(cancellationToken: cancellationToken);
    }
}
