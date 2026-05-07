using UnityEngine;
using VInspector;

public class DialogueObject : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _highlightObject;

    [SerializeField]
    private UI_Dialogue _uiDialogue;

    [Foldout("Project")]
    [SerializeField]
    private string _dialogueText;

    private bool _isUIVisible = false;

    protected override void HandlePlayerEnter()
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(true);
        }
    }

    protected override void HandlePlayerExit()
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(false);
        }
        
        if (_isUIVisible)
        {
            ToggleUI();
        }
    }

    protected override void Interact()
    {
        base.Interact();
        ToggleUI();
    }

    protected override void SetState(bool isOn)
    {
        base.SetState(isOn);
        if (_uiDialogue == null)
        {
            return;
        }

        if (isOn && !_isUIVisible)
        {
            _isUIVisible = true;
            _uiDialogue.ShowDialogue(_dialogueText);
        }
        else if (!isOn && _isUIVisible)
        {
            _isUIVisible = false;
            _uiDialogue.HideDialogue();
        }
    }

    private void ToggleUI()
    {
        SetState(!_isUIVisible);
    }
}
