using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NPCChoiceItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _choiceText;

    [SerializeField]
    private Button _choiceButton;

    private NPCEventSO _nextEvent;
    private Action<NPCEventSO> _onSelectedCallback;

    private void Awake()
    {
        if (_choiceButton != null)
        {
            _choiceButton.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Setup(NPCChoiceOption option, Action<NPCEventSO> onSelected)
    {
        if (_choiceText != null)
        {
            _choiceText.text = option.ChoiceText;
        }

        _nextEvent = option.NextEvent;
        _onSelectedCallback = onSelected;
    }

    private void OnButtonClicked()
    {
        _onSelectedCallback?.Invoke(_nextEvent);
    }
}
