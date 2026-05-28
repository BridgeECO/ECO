using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class UI_NPCChoice : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _choiceContainer;

    [SerializeField]
    private UI_NPCChoiceItem[] _choiceItems;

    public void Open(List<NPCChoiceOption> options, Action<NPCEventSO> onChoiceSelected)
    {
        if (options == null || options.Count == 0)
        {
            return;
        }

        if (_choiceContainer != null)
        {
            _choiceContainer.SetActive(true);
        }

        for (int i = 0; i < _choiceItems.Length; i++)
        {
            if (i < options.Count)
            {
                _choiceItems[i].gameObject.SetActive(true);
                _choiceItems[i].Setup(options[i], (nextEvent) =>
                {
                    Close();
                    onChoiceSelected?.Invoke(nextEvent);
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
        if (_choiceContainer != null)
        {
            _choiceContainer.SetActive(false);
        }

        for (int i = 0; i < _choiceItems.Length; i++)
        {
            _choiceItems[i].gameObject.SetActive(false);
        }
    }
}
