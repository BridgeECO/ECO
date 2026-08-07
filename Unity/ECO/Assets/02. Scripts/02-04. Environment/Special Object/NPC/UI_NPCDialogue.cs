using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;
using Cysharp.Threading.Tasks;

public class UI_NPCDialogue : MonoBehaviour
{
    // 외부(NPCEventFlowRunner)가 public 필드에 직접 대입하지 않도록 캡슐화한다.
    // SetDialogueCompletedCallback으로만 주입하며, Close 시 자동으로 해제된다.
    private Action _onDialogueCompleted;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_NPCDialogueAnimator _animator;

    [SerializeField]
    private UI_NPCDialogueTextBox _textBox;

    [SerializeField]
    private UI_NPCChoice _choiceUI;

    [Foldout("Settings")]
    [SerializeField]
    [Tooltip("대화 텍스트가 시작될 때 재생할 SFX. 캐릭터별로 다른 클립을 지정한다.")]
    private ESfxClip _dialogueClip;

    [SerializeField]
    [Tooltip("대화 SFX 발화를 활성화할지 여부.")]
    private bool _hasDialogueSfx = false;

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

    /// <summary>마지막 페이지에서 진행 입력 시 호출될 콜백을 지정한다. null로 해제한다.</summary>
    public void SetDialogueCompletedCallback(Action onCompleted)
    {
        _onDialogueCompleted = onCompleted;
    }

    public void Open(string[] lines)
    {
        if (lines is null || lines.Length == 0)
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

        if (_lines.Length - 1 <= _currentPageIndex)
        {
            _onDialogueCompleted?.Invoke();
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
        _onDialogueCompleted = null;
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
        _onDialogueCompleted = null;
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

        PlayDialogueSfx();
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

        PlayDialogueSfx();
        _isPrintingText = true;
        _isTransitioning = false;

        await _textBox.ShowPageAsync(_lines[_currentPageIndex], _currentPageIndex, _lines.Length);
        if (this == null || !IsDialogueOpen)
        {
            return;
        }

        _isPrintingText = false;
    }

    private void PlayDialogueSfx()
    {
        if (!_hasDialogueSfx || !SoundManager.HasInstance)
        {
            return;
        }
        SoundManager.Instance.PlayUiSfx(_dialogueClip);
    }
}
