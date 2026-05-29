using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;
using Cysharp.Threading.Tasks;

public class UI_NPCDialogue : MonoBehaviour
{
    public Action OnDialogueCompleted;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_NPCDialogueAnimator _animator;

    [SerializeField]
    private UI_NPCDialogueTextBox _textBox;

    [SerializeField]
    private UI_NPCChoice _choiceUI;

    private string[] _lines;
    private int _currentPageIndex;
    private bool _isShowingChoices;
    private bool _isTransitioning;
    private bool _isPrintingText;

    public bool IsDialogueOpen { get; private set; }

    private void Update()
    {
        if (!IsDialogueOpen || _isShowingChoices || _isTransitioning)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F) || (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())))
        {
            if (_isPrintingText)
            {
                _textBox.SkipPrinting();
            }
            else
            {
                AdvancePage();
            }
        }
    }

    public void Open(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        if (_choiceUI != null)
        {
            _choiceUI.Close();
        }

        _isShowingChoices = false;
        _lines = lines;
        _currentPageIndex = 0;
        IsDialogueOpen = true;

        OpenDialogueFlowAsync().Forget();
    }

    public void AdvancePage()
    {
        if (!IsDialogueOpen)
        {
            return;
        }

        if (_currentPageIndex >= _lines.Length - 1)
        {
            OnDialogueCompleted?.Invoke();
            return;
        }

        TransitionToNextPageAsync().Forget();
    }

    public void ShowChoices(List<NPCChoiceOption> options, Action<NPCEventSO> onChoiceSelected)
    {
        _isShowingChoices = true;
        if (_choiceUI != null)
        {
            _choiceUI.Open(options, onChoiceSelected);
        }
    }

    public void Close()
    {
        _isShowingChoices = false;
        _isTransitioning = false;
        _isPrintingText = false;
        if (!IsDialogueOpen)
        {
            return;
        }

        IsDialogueOpen = false;
        _animator.PlayFadeOut();

        if (_choiceUI != null)
        {
            _choiceUI.Close();
        }
    }

    public async UniTask CloseAsync()
    {
        _isShowingChoices = false;
        _isTransitioning = false;
        _isPrintingText = false;
        if (!IsDialogueOpen)
        {
            return;
        }

        IsDialogueOpen = false;
        await _animator.PlayFadeOutAsync();

        if (_choiceUI != null)
        {
            _choiceUI.Close();
        }
    }

    private async UniTaskVoid OpenDialogueFlowAsync()
    {
        _isTransitioning = true;
        await _animator.PlayFadeInAsync();
        if (this == null || !IsDialogueOpen)
        {
            return;
        }

        _isPrintingText = true;
        _isTransitioning = false;

        await _textBox.ShowPageAsync(_lines[_currentPageIndex], _currentPageIndex, _lines.Length);
        if (this == null || !IsDialogueOpen)
        {
            return;
        }

        _isPrintingText = false;
    }

    private async UniTaskVoid TransitionToNextPageAsync()
    {
        _isTransitioning = true;
        await _textBox.HideAsync();
        if (this == null || !IsDialogueOpen)
        {
            return;
        }

        _currentPageIndex++;
        
        _isPrintingText = true;
        _isTransitioning = false;

        await _textBox.ShowPageAsync(_lines[_currentPageIndex], _currentPageIndex, _lines.Length);
        if (this == null || !IsDialogueOpen)
        {
            return;
        }

        _isPrintingText = false;
    }
}
