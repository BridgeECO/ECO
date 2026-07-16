using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public abstract class UI_SystemPopup : UI_Popup
{
    [Foldout("UI")]
    [UnityEngine.Serialization.FormerlySerializedAs("UI_TitleText")]
    [SerializeField] 
    protected TextMeshProUGUI _uiTitleText;

    [UnityEngine.Serialization.FormerlySerializedAs("UI_MessageText")]
    [SerializeField] 
    protected TextMeshProUGUI _uiMessageText;

    // 파생 클래스에서 자신이 가진 버튼들을 리스트로 반환하도록 강제
    protected abstract List<Button> GetButtons();

    protected void SetPopupText(string title, string message)
    {
        if (_uiTitleText != null)
        {
            _uiTitleText.text = title;
        }
        if (_uiMessageText != null)
        {
            _uiMessageText.text = message;
        }
    }

    protected void ClearAllButtonListeners()
    {
        var buttons = GetButtons();
        if (buttons is null)
        {
            return;
        }
        
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }
        }
    }
}
