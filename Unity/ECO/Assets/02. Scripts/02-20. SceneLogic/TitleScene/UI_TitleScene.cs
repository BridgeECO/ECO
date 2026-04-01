using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TitleScene : MonoBehaviour
{
    [SerializeField]
    private TitleScene _titleScene;

    [SerializeField]
    private Image _teamLogoImage;

    [SerializeField]
    private Image _backgroundImage;

    [SerializeField]
    private CanvasGroup _pressAnyKeyCanvasGroup;

    [SerializeField]
    private CanvasGroup _buttonsCanvasGroup;


    [SerializeField]
    private TextMeshProUGUI _pressAnyKeyText;

    private void Awake()
    {
        InitUIState();

        _titleScene.OnIntroStarted += HandleIntro;
        _titleScene.OnWaitInputStarted += HandleWaitInput;
        _titleScene.OnMenuStarted += HandleMenu;
    }

    private void HandleIntro()
    {
        HandleIntroAsync().Forget();
    }

    private async UniTaskVoid HandleIntroAsync()
    {
        await _teamLogoImage.DOFade(1f, 0.5f).ToUniTask();
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        await _teamLogoImage.DOFade(0f, 0.5f).ToUniTask();
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        await _backgroundImage.DOFade(0f, 0.5f).ToUniTask();
    }

    private void HandleWaitInput()
    {
        HandleWaitInputAsync().Forget();
    }

    private async UniTaskVoid HandleWaitInputAsync()
    {
        await _pressAnyKeyCanvasGroup.DOFade(1f, 0.5f).ToUniTask();
        _pressAnyKeyText.Blink();
    }

    private void HandleMenu()
    {
        HandleMenuAsync().Forget();
    }

    private async UniTaskVoid HandleMenuAsync()
    {
        _pressAnyKeyText.DOKill();
        await _pressAnyKeyCanvasGroup.DOFade(0f, 0.5f).ToUniTask();
        await _buttonsCanvasGroup.DOFade(1f, 0.5f).ToUniTask();
    }

    private void InitUIState()
    {
        Color logoColor = _teamLogoImage.color;
        logoColor.a = 0f;
        _teamLogoImage.color = logoColor;

        Color bgColor = _backgroundImage.color;
        bgColor.a = 1f;
        _backgroundImage.color = bgColor;

        Color textColor = _pressAnyKeyText.color;
        textColor.a = 1f;
        _pressAnyKeyText.color = textColor;

        _pressAnyKeyCanvasGroup.alpha = 0f;
        _buttonsCanvasGroup.alpha = 0f;
    }
}