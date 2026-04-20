using UnityEngine;

public abstract class UI_SettingsTabBase : MonoBehaviour
{
    public abstract void Init();
    public abstract void Refresh();
    public abstract void ResetToDefault();
    public abstract void SaveSettings();
    public abstract bool HasUnsavedChanges();
}
