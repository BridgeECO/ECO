using DG.Tweening;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class UI_NPCDialogueTextBox : MonoBehaviour
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

    public async UniTask ShowPageAsync(string text, int currentIndex, int totalCount)
    {
        SetText(text);
        RefreshPageIndicator(currentIndex, totalCount);
        RefreshContinueIndicator(currentIndex, totalCount);
        SetTextAlpha(1f);
        await PlayTypewriterAsync();
    }

    public async UniTask HideAsync()
    {
        KillTypewriter();
        await PlayTextFadeOutAsync();
    }

    public void SkipPrinting()
    {
        if (_typewriterTween != null && _typewriterTween.IsActive())
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

        bool isLastPage = currentIndex >= totalCount - 1;
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
        if (_typewriterTween != null && _typewriterTween.IsActive())
        {
            _typewriterTween.Kill();
        }
        _typewriterTween = null;
    }

    private async UniTask PlayTypewriterAsync()
    {
        if (_dialogueText == null)
        {
            return;
        }

        KillTypewriter();

        _dialogueText.maxVisibleCharacters = 0;
        _dialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = _dialogueText.textInfo.characterCount;

        float duration = _textPrintSpeed > 0f ? totalVisibleCharacters / _textPrintSpeed : 0f;

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

        await _typewriterTween.ToUniTask();

        _dialogueText.maxVisibleCharacters = totalVisibleCharacters;
    }

    private async UniTask PlayTextFadeOutAsync()
    {
        if (_dialogueText == null)
        {
            return;
        }

        _dialogueText.DOKill();
        await _dialogueText.DOFade(0f, _fadeDuration).SetEase(Ease.InQuad).ToUniTask();
    }
}
