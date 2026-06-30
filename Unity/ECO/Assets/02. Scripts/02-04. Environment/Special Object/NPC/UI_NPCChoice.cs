using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class UI_NPCChoice : MonoBehaviour
{
    public Action<NPCEventSO> OnChoiceSelected;

    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _choiceContainer;

    [SerializeField]
    private UI_NPCChoiceItem[] _choiceItems;

    private List<NPCChoiceOption> _currentOptions;
    private bool _isOpen;

    private void Awake()
    {
        Close();
    }

    private void Update()
    {
        if (!_isOpen || _currentOptions == null)
        {
            return;
        }

        for (int i = 0; i < _currentOptions.Count; i++)
        {
            if (9<= i )
            {
                break;
            }
            
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                SelectChoice(i);
                break;
            }
        }
    }

    public void Open(List<NPCChoiceOption> options, Action<NPCEventSO> onChoiceSelected)
    {
        if (options == null || options.Count == 0)
        {
            return;
        }

        _currentOptions = options;
        OnChoiceSelected = onChoiceSelected;
        _isOpen = true;

        if (_choiceContainer != null)
        {
            _choiceContainer.SetActive(true);
        }

        for (int i = 0; i < _choiceItems.Length; i++)
        {
            if (i < options.Count)
            {
                _choiceItems[i].gameObject.SetActive(true);
                int index = i;
                _choiceItems[i].Setup(i, options[i], (nextEvent) =>
                {
                    SelectChoice(index);
                });
            }
            else
            {
                _choiceItems[i].gameObject.SetActive(false);
            }
        }
    }

    public void Close()
    {
        _isOpen = false;
        _currentOptions = null;
        OnChoiceSelected = null;

        if (_choiceContainer != null)
        {
            _choiceContainer.SetActive(false);
        }

        for (int i = 0; i < _choiceItems.Length; i++)
        {
            _choiceItems[i].gameObject.SetActive(false);
        }
    }

    private void SelectChoice(int index)
    {
        if (_currentOptions == null || index < 0 || _currentOptions.Count<= index )
        {
            return;
        }

        NPCEventSO nextEvent = _currentOptions[index].NextEvent;
        Action<NPCEventSO> callback = OnChoiceSelected;

        Close();
        callback?.Invoke(nextEvent);
    }
}
