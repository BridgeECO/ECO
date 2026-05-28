using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_NPCDialogue : MonoBehaviour
{
    public Action OnDialogueCompleted;

    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _dialogueCanvasGroup;

    [SerializeField]
    private TextMeshProUGUI _dialogueText;

    [SerializeField]
    private TextMeshProUGUI _pageIndicatorText;

    [SerializeField]
    private GameObject _continueIndicator;

    [SerializeField]
    private UI_NPCChoice _choiceUI;

    [Foldout("Settings")]
    [SerializeField]
    private float _fadeInDuration;

    [SerializeField]
    private float _fadeOutDuration;

    private string[] _lines;
    private int _currentPageIndex;

    public bool IsDialogueOpen { get; private set; }

    private void Awake()
    {
        InitCanvasGroup();
    }

    public void Open(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        _lines = lines;
        _currentPageIndex = 0;
        IsDialogueOpen = true;

        RefreshPage();
        PlayFadeIn();
    }

    public void AdvancePage()
    {
        if (!IsDialogueOpen)
        {
            return;
        }

        _currentPageIndex++;

        if (_currentPageIndex >= _lines.Length)
        {
            Close();
            OnDialogueCompleted?.Invoke();
            return;
        }

        RefreshPage();
    }

    public void ShowChoices(System.Collections.Generic.List<NPCChoiceOption> options, Action<NPCEventSO> onChoiceSelected)
    {
        if (_choiceUI != null)
        {
            _choiceUI.Open(options, onChoiceSelected);
        }
    }

    public void Close()
    {
        if (!IsDialogueOpen)
        {
            return;
        }

        IsDialogueOpen = false;
        PlayFadeOut();
        
        if (_choiceUI != null)
        {
            _choiceUI.Close();
        }
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

    private void RefreshPage()
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = _lines[_currentPageIndex];
        }

        RefreshPageIndicator();
        RefreshContinueIndicator();
    }

    private void RefreshPageIndicator()
    {
        if (_pageIndicatorText == null)
        {
            return;
        }

        if (_lines.Length <= 1)
        {
            _pageIndicatorText.gameObject.SetActive(false);
            return;
        }

        _pageIndicatorText.gameObject.SetActive(true);
        _pageIndicatorText.text = $"{_currentPageIndex + 1} / {_lines.Length}";
    }

    private void RefreshContinueIndicator()
    {
        if (_continueIndicator == null)
        {
            return;
        }

        bool isLastPage = _currentPageIndex >= _lines.Length - 1;
        _continueIndicator.SetActive(!isLastPage);
    }

    private void PlayFadeIn()
    {
        if (_dialogueCanvasGroup == null)
        {
            return;
        }

        _dialogueCanvasGroup.DOKill();
        _dialogueCanvasGroup.DOFade(1f, _fadeInDuration).SetEase(Ease.OutQuad);
    }

    private void PlayFadeOut()
    {
        if (_dialogueCanvasGroup == null)
        {
            return;
        }

        _dialogueCanvasGroup.DOKill();
        _dialogueCanvasGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.InQuad);
    }
}
