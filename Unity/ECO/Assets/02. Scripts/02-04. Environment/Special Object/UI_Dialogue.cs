using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VInspector;

public class UI_Dialogue : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _uiContainer;

    [SerializeField]
    private TextMeshProUGUI _dialogueTextDisplay;

    private void Awake()
    {
        HideDialogue();
    }

    public void ShowDialogue(string text)
    {
        if (!ReferenceEquals(_uiContainer, null))
        {
            _uiContainer.SetActive(true);
        }

        if (!ReferenceEquals(_dialogueTextDisplay, null))
        {
            _dialogueTextDisplay.text = text;
        }
    }

    public void HideDialogue()
    {
        if (!ReferenceEquals(_uiContainer, null))
        {
            _uiContainer.SetActive(false);
        }
    }
}
