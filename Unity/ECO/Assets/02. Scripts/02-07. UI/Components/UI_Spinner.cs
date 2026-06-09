using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 여러 개의 문자열 옵션 중 하나를 선택할 수 있는 스피너 UI 컴포넌트.
/// 좌우 화살표 버튼으로 옵션을 변경하고, OnValueChanged 이벤트를 통해 변경 사항을 알린다.
/// </summary>
public class UI_Spinner : MonoBehaviour
{
    public Action<int, string> OnValueChanged;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _prevBtn;

    [SerializeField]
    private Button _nextBtn;

    [SerializeField]
    private TextMeshProUGUI _optionText;

    [Foldout("Config")]
    [SerializeField]
    private List<string> _options = new List<string>();

    private int _currentIndex;

    public int CurrentIndex => _currentIndex;
    public string CurrentOption => _options != null && _options.Count > 0 ? _options[_currentIndex] : string.Empty;

    private void Awake()
    {
        if (_prevBtn != null)
        {
            _prevBtn.onClick.AddListener(() => ChangeOption(-1));
        }

        if (_nextBtn != null)
        {
            _nextBtn.onClick.AddListener(() => ChangeOption(1));
        }
    }

    private void Start()
    {
        RefreshText();
    }

    public void SetOptions(List<string> options)
    {
        _options = options;
        if (_options != null && _options.Count > 0)
        {
            _currentIndex = Mathf.Clamp(_currentIndex, 0, _options.Count - 1);
            RefreshText();
        }
    }

    public void SetValueWithoutNotify(int index)
    {
        if (_options == null || _options.Count == 0)
        {
            return;
        }

        _currentIndex = Mathf.Clamp(index, 0, _options.Count - 1);
        RefreshText();
    }

    private void ChangeOption(int direction)
    {
        if (_options == null || _options.Count == 0)
        {
            return;
        }

        _currentIndex = (_currentIndex + direction + _options.Count) % _options.Count;
        RefreshText();
        OnValueChanged?.Invoke(_currentIndex, _options[_currentIndex]);
    }

    private void RefreshText()
    {
        if (_optionText != null && _options != null && _options.Count > 0)
        {
            _optionText.text = _options[_currentIndex];
        }
    }
}
