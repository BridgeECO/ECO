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

    private void OnDestroy()
    {
        _respawnButton.onClick.RemoveListener(OnClickRespawn);
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
