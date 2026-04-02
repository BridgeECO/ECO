using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Buttons")]
    [SerializeField] private Button _startButton;

    private void Awake()
    {
        InitUIState();
        _titleScene.OnIntroStarted += HandleIntro;
        _titleScene.OnWaitInputStarted += HandleWaitInput;
        _titleScene.OnMenuStarted += HandleMenu;

        _startButton.onClick.AddListener(OnClickStart);
    }

    private void OnDestroy()
    {
        _titleScene.OnIntroStarted -= HandleIntro;
        _titleScene.OnWaitInputStarted -= HandleWaitInput;
        _titleScene.OnMenuStarted -= HandleMenu;

        _startButton.onClick.RemoveListener(OnClickStart);
    }

    private void HandleIntro()
    {
        HandleIntroAsync().Forget();
    }

    private async UniTaskVoid HandleIntroAsync()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        await _teamLogoImage.DOFade(1f, 1f).ToUniTask(cancellationToken: ct);
        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: ct);
        await _teamLogoImage.DOFade(0f, 1f).ToUniTask(cancellationToken: ct);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);
        await _backgroundImage.DOFade(0f, 0.5f).ToUniTask(cancellationToken: ct);
    }

    private void HandleWaitInput()
    {
        HandleWaitInputAsync().Forget();
    }

    private async UniTaskVoid HandleWaitInputAsync()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        await _pressAnyKeyCanvasGroup.DOFade(1f, 0.5f).ToUniTask(cancellationToken: ct);
        _pressAnyKeyText.Blink();
    }

    private void HandleMenu()
    {
        HandleMenuAsync().Forget();
    }

    private async UniTaskVoid HandleMenuAsync()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        _pressAnyKeyText.DOKill();
        await _pressAnyKeyCanvasGroup.DOFade(0f, 0.5f).ToUniTask(cancellationToken: ct);
        await _buttonsCanvasGroup.DOFade(1f, 0.5f).ToUniTask(cancellationToken: ct);
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

    private void OnClickStart()
    {
        if (_buttonsCanvasGroup.alpha < 0.9f) return;

        SceneManager.LoadScene("PersistentScene", LoadSceneMode.Single);
    }
}