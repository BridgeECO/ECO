using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class UI_NPCDialogueAnimator : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _dialogueCanvasGroup;

    [SerializeField]
    private float _fadeDuration;

    private void Awake()
    {
        InitCanvasGroup();
    }

    public void PlayFadeIn()
    {
        if (_dialogueCanvasGroup == null)
        {
            return;
        }

        _dialogueCanvasGroup.DOKill();
        _dialogueCanvasGroup.interactable = true;
        _dialogueCanvasGroup.blocksRaycasts = true;
        _dialogueCanvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.OutQuad);
    }

    public async UniTask PlayFadeInAsync()
    {
        if (_dialogueCanvasGroup == null)
        {
            return;
        }

        _dialogueCanvasGroup.DOKill();
        _dialogueCanvasGroup.interactable = true;
        _dialogueCanvasGroup.blocksRaycasts = true;
        await _dialogueCanvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.OutQuad).ToUniTask();
    }

    public void PlayFadeOut()
    {
        if (_dialogueCanvasGroup == null)
        {
            return;
        }

        _dialogueCanvasGroup.DOKill();
        _dialogueCanvasGroup.interactable = false;
        _dialogueCanvasGroup.blocksRaycasts = false;
        _dialogueCanvasGroup.DOFade(0f, _fadeDuration).SetEase(Ease.InQuad);
    }

    public async UniTask PlayFadeOutAsync()
    {
        if (_dialogueCanvasGroup == null)
        {
            return;
        }

        _dialogueCanvasGroup.DOKill();
        _dialogueCanvasGroup.interactable = false;
        _dialogueCanvasGroup.blocksRaycasts = false;
        await _dialogueCanvasGroup.DOFade(0f, _fadeDuration).SetEase(Ease.InQuad).ToUniTask();
    }

    private void InitCanvasGroup()
    {
        if (_dialogueCanvasGroup == null)
        {
            return;
        }

        _dialogueCanvasGroup.alpha = 0f;
        _dialogueCanvasGroup.interactable = false;
        _dialogueCanvasGroup.blocksRaycasts = false;
    }
}
