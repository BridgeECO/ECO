using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Control : UI_SettingsTabBase
{
    [Foldout("UI")]
    [SerializeField] private TMP_Dropdown UI_InputDeviceDropdown;
    [SerializeField] private Slider UI_MouseSensitivitySlider;
    [SerializeField] private Slider UI_InvertYSwitch; // Slider -> Switch for inspector clarity
    [SerializeField] private Slider UI_VibrationSwitch; // Slider -> Switch for inspector clarity
    [SerializeField] private Button UI_RebindJumpButton;
    [SerializeField] private TextMeshProUGUI UI_JumpKeyText;

    private bool isDirty;
    private CancellationTokenSource cts;

    public override void Init()
    {
        UI_InputDeviceDropdown.onValueChanged.AddListener(val => SetDirty());
        UI_MouseSensitivitySlider.onValueChanged.AddListener(val => SetDirty());
        UI_InvertYSwitch.onValueChanged.AddListener(val => SetDirty());
        UI_VibrationSwitch.onValueChanged.AddListener(val => SetDirty());

        UI_RebindJumpButton.onClick.AddListener(OnClick_RebindJump);

        cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    public override void Refresh()
    {
        isDirty = false;
    }

    public override void ResetToDefault()
    {
        UI_MouseSensitivitySlider.value = 0.5f;
        UI_InvertYSwitch.value = 0f;
        UI_VibrationSwitch.value = 1f;
        SetDirty();
    }

    public override void SaveSettings()
    {
        isDirty = false;
    }

    public override bool HasUnsavedChanges()
    {
        return isDirty;
    }

    private void SetDirty()
    {
        isDirty = true;
    }

    private void OnClick_RebindJump()
    {
        WaitForKeyInputAsync(cts.Token).Forget();
    }

    private async UniTaskVoid WaitForKeyInputAsync(CancellationToken token)
    {
        UI_JumpKeyText.text = "Press Any Key...";

        bool keyBound = false;

        while (!keyBound)
        {
            await UniTask.Yield(token);

            if (UnityEngine.Input.anyKeyDown)
            {
                UI_JumpKeyText.text = "Space";
                keyBound = true;
                SetDirty();
            }
        }
    }
}
