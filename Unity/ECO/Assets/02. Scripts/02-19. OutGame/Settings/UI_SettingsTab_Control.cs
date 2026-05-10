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

    [SerializeField]
    private Button _rebindInteractionButton;

    [SerializeField]
    private TextMeshProUGUI _interactionKeyText;

    private static readonly KeyCode[] _allKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    private bool _isDirty;
    private bool _isInitialized;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        InitTab();
    }

    public override void InitTab()
    {
        if (_isInitialized)
        {
            return;
        }

        _inputDeviceDropdown.onValueChanged.AddListener(val => SetDirty());
        _mouseSensitivitySlider.onValueChanged.AddListener(val => SetDirty());
        _invertYSwitch.onValueChanged.AddListener(val => SetDirty());
        _vibrationSwitch.onValueChanged.AddListener(val => SetDirty());
        _rebindJumpButton.onClick.AddListener(OnClick_RebindJump);
        _rebindInteractionButton.onClick.AddListener(OnClick_RebindInteraction);
        _cts = new CancellationTokenSource();

        _isInitialized = true;
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
        WaitForKeyInputAsync(_jumpKeyText).Forget();
    }

    private void OnClick_RebindInteraction()
    {
        WaitForKeyInputAsync(_interactionKeyText).Forget();
    }

    private async UniTaskVoid WaitForKeyInputAsync(TextMeshProUGUI targetText)
    {
        string originalText = targetText.text;
        targetText.text = "Press Any Key...";

        await UniTask.Yield(_cts.Token);

        bool keyBound = false;

        while (!keyBound)
        {
            await UniTask.Yield(_cts.Token);
            if (Input.anyKeyDown && !Input.GetMouseButtonDown(0))
            {
                for (int i = 0; i < _allKeyCodes.Length; i++)
                {
                    if (Input.GetKeyDown(_allKeyCodes[i]))
                    {
                        targetText.text = _allKeyCodes[i].ToString();
                        keyBound = true;
                        SetDirty();
                        break;
                    }
                }
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
