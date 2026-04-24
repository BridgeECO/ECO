using UnityEngine;

public abstract class UI_SettingsTabBase : MonoBehaviour
{
    public abstract void InitTab();
    public abstract void RefreshTab();
    public abstract void ResetTabToDefault();
    public abstract void SaveTabSettings();
    public abstract bool HasUnsavedChanges();
}
