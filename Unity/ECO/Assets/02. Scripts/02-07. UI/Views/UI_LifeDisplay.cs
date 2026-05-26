using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LifeDisplay : MonoBehaviour
{
    [SerializeField]
    private List<Image> _lifeIcons;

    private void Start()
    {
        EventManager.Instance.AddEventListener(EEventType.LifeChanged, RefreshLifeDisplay);
        RefreshLifeDisplay();
    }

    private void OnDestroy()
    {
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.LifeChanged, RefreshLifeDisplay);
        }
    }

    private void RefreshLifeDisplay()
    {
        int currentLife = LifeManager.Instance.CurrentLife;
        for (int i = 0; i < _lifeIcons.Count; i++)
        {
            if (_lifeIcons[i] == null)
            {
                continue;
            }
            _lifeIcons[i].enabled = i < currentLife;
        }
    }
}
