using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// UI_ClickSlider의 OnValueChanged 이벤트를 구독하여 값을 TMP 텍스트로 표시하는 클래스.
/// 소수점 자릿수와 표시 배율을 조정할 수 있다. (예: 0~1 값을 0~100%로 표시)
/// </summary>
public class UI_SliderValueText : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_ClickSlider _slider;

    [Foldout("Config")]
    [Header("Display")]
    [SerializeField]
    private float _displayMultiplier = 100f;

    [SerializeField]
    private int _decimalPlaces = 0;

    [SerializeField]
    private string _suffix = "";

    private TextMeshProUGUI _valueText;

    private void Awake()
    {
        // 자신과 같은 GameObject에 붙어 있는 TMP를 자동으로 찾는다.
        _valueText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (_slider != null)
        {
            _slider.OnValueChanged += RefreshText;
            _slider.OnVisualsUpdated += RefreshText;
        }

        RefreshText(_slider != null ? _slider.Value : 0f);
    }

    private void OnDisable()
    {
        if (_slider != null)
        {
            _slider.OnValueChanged -= RefreshText;
            _slider.OnVisualsUpdated -= RefreshText;
        }
    }

    private void RefreshText(float value)
    {
        if (_valueText == null)
        {
            return;
        }

        float displayValue = value * _displayMultiplier;
        string format = "F" + _decimalPlaces;
        _valueText.text = displayValue.ToString(format) + _suffix;
    }
}

