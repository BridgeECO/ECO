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
        if (!ReferenceEquals(_highlightObject, null))
        {
            _highlightObject.SetActive(true);
        }
    }

    protected override void HandlePlayerExit()
    {
        if (!ReferenceEquals(_highlightObject, null))
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

    private void ToggleUI()
    {
        if (ReferenceEquals(_uiDialogue, null))
        {
            return;
        }

        _isUIVisible = !_isUIVisible;
        
        if (_isUIVisible)
        {
            _uiDialogue.ShowDialogue(_dialogueText);
        }
        else
        {
            _uiDialogue.HideDialogue();
        }
    }
}
