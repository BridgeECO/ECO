using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class UI_SettingsTab_Graphics : UI_SettingsTabBase
{
    [Foldout("Hierarchy - Spinners")]
    [SerializeField]
    private UI_Spinner _resolutionSpinner;

    [SerializeField]
    private UI_Spinner _displayModeSpinner;

    [SerializeField]
    private UI_Spinner _screenShakingSpinner;

    [Foldout("Hierarchy - Sliders")]
    [SerializeField]
    private UI_ClickSlider _brightnessSlider;

    [SerializeField]
    private UI_ClickSlider _contrastSlider;

    [Foldout("Config")]
    [Header("해상도 목록")]
    [SerializeField]
    private List<string> _resolutionOptions = new List<string> { "1280 * 720", "1920 * 1080", "2560 * 1440", "3840 * 2160" };

    [Header("디스플레이 모드 목록")]
    [SerializeField]
    private List<string> _displayModeOptions = new List<string> { "창 모드", "전체 화면" };

    [Header("화면 흔들림 목록")]
    [SerializeField]
    private List<string> _screenShakingOptions = new List<string> { "끄기", "켜기" };

    private bool _isInitialized;

    // 현재 설정/적용 중인 임시 값들
    private int _currentResolutionIndex;
    private int _currentDisplayModeIndex;
    private int _currentScreenShakingIndex;
    private float _currentBrightness;
    private float _currentContrast;

    // PlayerPrefs 키
    private const string PrefKey_Resolution = "Settings_Graphics_Resolution";
    private const string PrefKey_DisplayMode = "Settings_Graphics_DisplayMode";
    private const string PrefKey_ScreenShaking = "Settings_Graphics_ScreenShaking";
    private const string PrefKey_Brightness = "Settings_Graphics_Brightness";
    private const string PrefKey_Contrast = "Settings_Graphics_Contrast";

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
            ApplyAllSettings();
            IsDirty = false;
        }
    }

    public override void InitTab()
    {
        if (_isInitialized)
        {
            return;
        }

        if (_resolutionSpinner != null)
        {
            _resolutionSpinner.SetOptions(_resolutionOptions);
        }
        if (_displayModeSpinner != null)
        {
            _displayModeSpinner.SetOptions(_displayModeOptions);
        }
        if (_screenShakingSpinner != null)
        {
            _screenShakingSpinner.SetOptions(_screenShakingOptions);
        }

        LoadSavedSettings();
        SyncUIToCurrentValues();

        if (_resolutionSpinner != null)
        {
            _resolutionSpinner.OnValueChanged += (index, option) =>
            {
                _currentResolutionIndex = index;
                ApplyResolutionImmediately();
                IsDirty = true;
            };
        }

        if (_displayModeSpinner != null)
        {
            _displayModeSpinner.OnValueChanged += (index, option) =>
            {
                _currentDisplayModeIndex = index;
                ApplyDisplayModeImmediately();
                IsDirty = true;
            };
        }

        if (_screenShakingSpinner != null)
        {
            _screenShakingSpinner.OnValueChanged += (index, option) =>
            {
                _currentScreenShakingIndex = index;
                ApplyScreenShakingImmediately();
                IsDirty = true;
            };
        }

        if (_brightnessSlider != null)
        {
            _brightnessSlider.OnValueChanged += val =>
            {
                _currentBrightness = val;
                ApplyBrightnessImmediately();
                IsDirty = true;
            };
        }

        if (_contrastSlider != null)
        {
            _contrastSlider.OnValueChanged += val =>
            {
                _currentContrast = val;
                ApplyContrastImmediately();
                IsDirty = true;
            };
        }

        _isInitialized = true;
    }

    private void LoadSavedSettings()
    {
        _currentResolutionIndex = PlayerPrefs.GetInt(PrefKey_Resolution, 1); // 기본값: 1920*1080
        _currentDisplayModeIndex = PlayerPrefs.GetInt(PrefKey_DisplayMode, 1); // 기본값: 전체 화면
        _currentScreenShakingIndex = PlayerPrefs.GetInt(PrefKey_ScreenShaking, 1); // 기본값: 켜기
        _currentBrightness = PlayerPrefs.GetFloat(PrefKey_Brightness, 1f);
        _currentContrast = PlayerPrefs.GetFloat(PrefKey_Contrast, 1f);
    }

    private void SyncUIToCurrentValues()
    {
        if (_resolutionSpinner != null)
        {
            _resolutionSpinner.SetValueWithoutNotify(_currentResolutionIndex);
        }
        if (_displayModeSpinner != null)
        {
            _displayModeSpinner.SetValueWithoutNotify(_currentDisplayModeIndex);
        }
        if (_screenShakingSpinner != null)
        {
            _screenShakingSpinner.SetValueWithoutNotify(_currentScreenShakingIndex);
        }

        if (_brightnessSlider != null)
        {
            _brightnessSlider.SetValueWithoutNotify(_currentBrightness);
        }
        if (_contrastSlider != null)
        {
            _contrastSlider.SetValueWithoutNotify(_currentContrast);
        }
    }

    private void ApplyAllSettings()
    {
        ApplyResolutionImmediately();
        ApplyDisplayModeImmediately();
        ApplyScreenShakingImmediately();
        ApplyBrightnessImmediately();
        ApplyContrastImmediately();
    }

    private void ApplyResolutionImmediately()
    {
        if (_currentResolutionIndex >= 0 && _currentResolutionIndex < _resolutionOptions.Count)
        {
            string resStr = _resolutionOptions[_currentResolutionIndex];
            string[] split = resStr.Replace(" ", "").Split('*');
            if (split.Length == 2 && int.TryParse(split[0], out int width) && int.TryParse(split[1], out int height))
            {
                FullScreenMode mode = _currentDisplayModeIndex == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                Screen.SetResolution(width, height, mode);
            }
        }
    }

    private void ApplyDisplayModeImmediately()
    {
        FullScreenMode mode = _currentDisplayModeIndex == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;
    }

    private void ApplyScreenShakingImmediately()
    {
        // 화면 흔들림 로직 연동 (예: CameraManager나 PlayerPrefs 읽어오기 활용)
        bool isShakeOn = _currentScreenShakingIndex == 1;
        // Debug.Log($"Screen Shaking Applied: {isShakeOn}");
    }

    private void ApplyBrightnessImmediately()
    {
        // 밝기 로직 연동 (PostProcessing Volume 등)
        // Debug.Log($"Brightness Applied: {_currentBrightness}");
    }

    private void ApplyContrastImmediately()
    {
        // 대비 로직 연동 (PostProcessing Volume 등)
        // Debug.Log($"Contrast Applied: {_currentContrast}");
    }

    public override void RefreshTab()
    {
        // 탭 전환 시 저장되지 않은 변경사항을 유지하도록 LoadSavedSettings 제외
        SyncUIToCurrentValues();
    }

    public override void ResetTabToDefault()
    {
        _currentResolutionIndex = 1;
        _currentDisplayModeIndex = 1;
        _currentScreenShakingIndex = 1;
        _currentBrightness = 1f;
        _currentContrast = 1f;

        SyncUIToCurrentValues();
        ApplyAllSettings();

        IsDirty = true;
    }

    public override void SaveTabSettings()
    {
        PlayerPrefs.SetInt(PrefKey_Resolution, _currentResolutionIndex);
        PlayerPrefs.SetInt(PrefKey_DisplayMode, _currentDisplayModeIndex);
        PlayerPrefs.SetInt(PrefKey_ScreenShaking, _currentScreenShakingIndex);
        PlayerPrefs.SetFloat(PrefKey_Brightness, _currentBrightness);
        PlayerPrefs.SetFloat(PrefKey_Contrast, _currentContrast);
        PlayerPrefs.Save();

        IsDirty = false;
    }

}
