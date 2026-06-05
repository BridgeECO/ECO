using UnityEngine;

public abstract class UI_Popup : MonoBehaviour
{
    [SerializeField]
    private float _destroyTime = 0.5f;

    public float DestroyTime => _destroyTime;

    public virtual void InitPopup()
    {
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnCloseButtonClicked()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
    }
}
