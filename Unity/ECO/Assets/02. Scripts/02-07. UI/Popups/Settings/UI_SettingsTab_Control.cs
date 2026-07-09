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
    private bool _isInitialized;
    private CancellationTokenSource _cts;

    private int _currentInputDeviceIndex;
    private float _currentMouseSensitivity;
    private float _currentInvertY;
    private float _currentVibration;

    private const string PrefKey_InputDevice = "Settings_Control_InputDevice";
    private const string PrefKey_MouseSensitivity = "Settings_Control_MouseSensitivity";
    private const string PrefKey_InvertY = "Settings_Control_InvertY";
    private const string PrefKey_Vibration = "Settings_Control_Vibration";

    private void Awake()
    {
        InitTab();
    }

    private void OnEnable()
    {
        if (_isInitialized)
        {
            SyncUIToCurrentValues();
        }
    }

    private void OnDisable()
    {
        // 팝업이 닫힐 때(적용 버튼을 누르지 않고 무시했을 경우 등) 원래의 저장된 상태로 복원
        if (IsDirty)
        {
            LoadSavedSettings();
            ApplySettingsImmediately();
            IsDirty = false;
        }
    }

    public override void InitTab()
    {
        if (_isInitialized)
        {
            return;
        }

        LoadSavedSettings();
        SyncUIToCurrentValues();

        if (_inputDeviceDropdown != null)
        {
            _inputDeviceDropdown.onValueChanged.AddListener(val => { _currentInputDeviceIndex = val; ApplySettingsImmediately(); IsDirty = true; });
        }
        if (_mouseSensitivitySlider != null)
        {
            _mouseSensitivitySlider.onValueChanged.AddListener(val => { _currentMouseSensitivity = val; ApplySettingsImmediately(); IsDirty = true; });
        }
        if (_invertYSwitch != null)
        {
            _invertYSwitch.onValueChanged.AddListener(val => { _currentInvertY = val; ApplySettingsImmediately(); IsDirty = true; });
        }
        if (_vibrationSwitch != null)
        {
            _vibrationSwitch.onValueChanged.AddListener(val => { _currentVibration = val; ApplySettingsImmediately(); IsDirty = true; });
        }
        if (_rebindJumpButton != null)
        {
            _rebindJumpButton.onClick.AddListener(OnClick_RebindJump);
        }
        if (_rebindInteractionButton != null)
        {
            _rebindInteractionButton.onClick.AddListener(OnClick_RebindInteraction);
        }
        _cts = new CancellationTokenSource();

        _isInitialized = true;
    }

    private void LoadSavedSettings()
    {
        _currentInputDeviceIndex = PlayerPrefs.GetInt(PrefKey_InputDevice, 0);
        _currentMouseSensitivity = PlayerPrefs.GetFloat(PrefKey_MouseSensitivity, 0.5f);
        _currentInvertY = PlayerPrefs.GetFloat(PrefKey_InvertY, 0f);
        _currentVibration = PlayerPrefs.GetFloat(PrefKey_Vibration, 1f);
    }

    private void SyncUIToCurrentValues()
    {
        if (_inputDeviceDropdown != null)
        {
            _inputDeviceDropdown.SetValueWithoutNotify(_currentInputDeviceIndex);
        }
        if (_mouseSensitivitySlider != null)
        {
            _mouseSensitivitySlider.SetValueWithoutNotify(_currentMouseSensitivity);
        }
        if (_invertYSwitch != null)
        {
            _invertYSwitch.SetValueWithoutNotify(_currentInvertY);
        }
        if (_vibrationSwitch != null)
        {
            _vibrationSwitch.SetValueWithoutNotify(_currentVibration);
        }
    }

    private void ApplySettingsImmediately()
    {
        // 실제 컨트롤 매니저와 연동하는 코드 작성 필요
    }

    public override void RefreshTab()
    {
        SyncUIToCurrentValues();
    }

    public override void ResetTabToDefault()
    {
        _currentInputDeviceIndex = 0;
        _currentMouseSensitivity = 0.5f;
        _currentInvertY = 0f;
        _currentVibration = 1f;

        SyncUIToCurrentValues();
        ApplySettingsImmediately();
        IsDirty = true;
    }

    public override void SaveTabSettings()
    {
        PlayerPrefs.SetInt(PrefKey_InputDevice, _currentInputDeviceIndex);
        PlayerPrefs.SetFloat(PrefKey_MouseSensitivity, _currentMouseSensitivity);
        PlayerPrefs.SetFloat(PrefKey_InvertY, _currentInvertY);
        PlayerPrefs.SetFloat(PrefKey_Vibration, _currentVibration);
        PlayerPrefs.Save();

        IsDirty = false;
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
                        IsDirty = true;
                        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Setting_ChangeKey);
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
