using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_InGame : MonoBehaviour
{
    [Foldout("Hierarchy")]
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
        LifeManager.Instance.InstantKill();
    }
}
