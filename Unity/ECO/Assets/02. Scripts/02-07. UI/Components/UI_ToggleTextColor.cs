using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// Toggle의 선택 상태(isOn)에 따라 지정된 TextMeshProUGUI의 텍스트 색상을 변경하는 클래스.
/// 활성화 상태(노란색)와 비활성화 상태 색상을 관리한다.
/// </summary>
public class UI_ToggleTextColor : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private TextMeshProUGUI _text;

    [Foldout("Config")]
    [Header("Colors")]
    [SerializeField]
    private Color _activeColor = new Color(1f, 0.85f, 0.2f, 1f);

    [SerializeField]
    private Color _inactiveColor = new Color(1f, 1f, 1f, 1f);

    private void Awake()
    {
        if (_toggle == null)
        {
            _toggle = GetComponent<Toggle>();
        }

        if (_text == null)
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        if (_toggle != null)
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            RefreshColor(_toggle.isOn);
        }
        else
        {
            RefreshColor(false);
        }
    }

    private void OnDisable()
    {
        if (_toggle != null)
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        RefreshColor(isOn);
    }

    /// <summary>
    /// 현재 Toggle 상태에 맞춰 텍스트 색상을 반영한다.
    /// </summary>
    /// <param name="isOn">Toggle 선택 여부</param>
    public void RefreshColor(bool isOn)
    {
        if (_text == null)
        {
            return;
        }

        _text.color = isOn ? _activeColor : _inactiveColor;
    }
}
