using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    [SerializeField]
    private Button _respawnButton;

    private void Start()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        _respawnButton.onClick.AddListener(OnClickRespawn);
    }

    private void OnClickRespawn()
    {
        EventManager.Instance.BroadcastEvent(EEventType.RespawnRequested);
    }
}
