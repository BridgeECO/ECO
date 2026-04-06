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

