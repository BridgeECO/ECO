using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NPCChoiceItem : MonoBehaviour
{
    public Action<NPCEventSO> OnSelectedCallback;

    [SerializeField]
    private TextMeshProUGUI _choiceText;

    [SerializeField]
    private Button _choiceButton;

    private NPCEventSO _nextEvent;

    private void Awake()
    {
        if (_choiceButton != null)
        {
            _choiceButton.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Setup(int index, NPCChoiceOption option, Action<NPCEventSO> onSelected)
    {
        if (_choiceText != null)
        {
            _choiceText.text = $"{index + 1}. {option.ChoiceText}";
        }

        _nextEvent = option.NextEvent;
        OnSelectedCallback = onSelected;
    }

    private void OnButtonClicked()
    {
        OnSelectedCallback?.Invoke(_nextEvent);
    }
}
