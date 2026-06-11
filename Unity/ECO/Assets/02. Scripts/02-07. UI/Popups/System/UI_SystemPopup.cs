using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;
using Ricimi;

public abstract class UI_SystemPopup : UI_Popup
{
    [Foldout("UI")]
    [UnityEngine.Serialization.FormerlySerializedAs("UI_TitleText")]
    [SerializeField] 
    private TextMeshProUGUI _uiTitleText;

    [UnityEngine.Serialization.FormerlySerializedAs("UI_MessageText")]
    [SerializeField] 
    private TextMeshProUGUI _uiMessageText;

    protected void SetPopupText(string title, string message)
    {
        if (_uiTitleText != null) _uiTitleText.text = title;
        if (_uiMessageText != null) _uiMessageText.text = message;
    }


}
