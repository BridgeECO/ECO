using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _loadingPanel;

    [SerializeField]
    private UI_PauseMenuPopup _popupPauseMenu;

    private void Start()
    {
        InputHandler.OnCancelEvent += HandleCancelInput;
    }

    private void OnDisable()
    {
        InputHandler.OnCancelEvent -= HandleCancelInput;
    }

    private void HandleCancelInput()
    {
        if (UI_PopupHandler.Instance != null && !UI_PopupHandler.Instance.HasPopups)
        {
            if (_popupPauseMenu != null)
            {
                UI_PopupHandler.Instance.OpenPopup(_popupPauseMenu);
            }
        }
    }

    public void FadeInLoadingPanel(Action onComplete = null)
    {
        _loadingPanel.DOFade(1f, 1f).SetEase(Ease.InQuad)
        .OnComplete(() => onComplete?.Invoke());
    }

    public void FadeOutLoadingPanel(Action onComplete = null)
    {
        _loadingPanel.DOFade(0f, 1f).SetEase(Ease.OutQuad)
        .OnComplete(() => onComplete?.Invoke());
    }
}

