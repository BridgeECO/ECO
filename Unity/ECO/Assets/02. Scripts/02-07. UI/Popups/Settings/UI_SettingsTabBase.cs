using UnityEngine;

public abstract class UI_SettingsTabBase : MonoBehaviour
{
    public bool IsDirty { get; protected set; }

    public abstract void InitTab();
    public abstract void RefreshTab();
    public abstract void ResetTabToDefault();
    public abstract void SaveTabSettings();

    public virtual bool HasUnsavedChanges()
    {
        return IsDirty;
    }
}
