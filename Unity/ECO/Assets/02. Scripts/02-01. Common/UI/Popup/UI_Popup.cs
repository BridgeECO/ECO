using UnityEngine;

public abstract class UI_Popup : MonoBehaviour
{
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
        UI_PopupHandler.Instance.ClosePopup(this);
    }
}
