using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Control : UI_SettingsTabBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Dropdown _inputDeviceDropdown;

    [SerializeField]
    private Slider _mouseSensitivitySlider;

    [SerializeField]
    private Slider _invertYSwitch;

    [SerializeField]
    private Slider _vibrationSwitch;

    [SerializeField]
    private Button _rebindJumpButton;

    [SerializeField]
    private TextMeshProUGUI _jumpKeyText;

    private bool _isDirty;
    private CancellationTokenSource _cts;

    public override void InitTab()
    {
        _inputDeviceDropdown.onValueChanged.AddListener(val => SetDirty());
        _mouseSensitivitySlider.onValueChanged.AddListener(val => SetDirty());
        _invertYSwitch.onValueChanged.AddListener(val => SetDirty());
        _vibrationSwitch.onValueChanged.AddListener(val => SetDirty());
        _rebindJumpButton.onClick.AddListener(OnClick_RebindJump);
        _cts = new CancellationTokenSource();
    }

    public override void RefreshTab()
    {
        _isDirty = false;
    }

    public override void ResetTabToDefault()
    {
        _mouseSensitivitySlider.value = 0.5f;
        _invertYSwitch.value = 0f;
        _vibrationSwitch.value = 1f;
        SetDirty();
    }

    public override void SaveTabSettings()
    {
        _isDirty = false;
    }

    public override bool HasUnsavedChanges()
    {
        return _isDirty;
    }

    private void SetDirty()
    {
        _isDirty = true;
    }

    private void OnClick_RebindJump()
    {
        WaitForKeyInputAsync(_cts.Token).Forget();
    }

    private async UniTaskVoid WaitForKeyInputAsync(CancellationToken token)
    {
        _jumpKeyText.text = "Press Any Key...";

        bool keyBound = false;

        while (!keyBound)
        {
            await UniTask.Yield(token);

            if (UnityEngine.Input.anyKeyDown)
            {
                _jumpKeyText.text = "Space";
                keyBound = true;
                SetDirty();
            }
        }
    }

    private void OnDestroy()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
