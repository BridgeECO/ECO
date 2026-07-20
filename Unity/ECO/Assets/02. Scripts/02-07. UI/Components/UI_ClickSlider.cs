using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 핸들 없이 클릭/드래그 지점의 위치 비율로 슬라이더 값을 제어하는 커스텀 슬라이더.
/// Unity Slider 컴포넌트 대신 사용하며, Fill Image 하나와 클릭 가능한 배경 RectTransform만으로 동작한다.
/// </summary>
public class UI_ClickSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Action<float> OnValueChanged;
    public Action<float> OnVisualsUpdated;

    /// <summary>슬라이더 조작을 완료하고 마우스 버튼을 뗐을 때 1회 발생하는 이벤트.</summary>
    public Action OnPointerUp;

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _sliderBackground;

    [SerializeField]
    private Image _fillImage;

    [Foldout("Config")]
    [SerializeField]
    private float _minValue = 0f;

    [SerializeField]
    private float _maxValue = 1f;

    [SerializeField]
    private float _currentValue = 0f;

    private Camera _uiCamera;

    public float Value
    {
        get => _currentValue;
        set => SetValue(value);
    }

    private void Awake()
    {
        // Canvas가 Screen Space - Camera 모드일 경우 UI 카메라를 캐싱한다.
        // Overlay 모드인 경우 null이 되므로 null 상태 작동을 허용한다.
        Canvas canvas = GetComponentInParent<Canvas>();
        _uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera
            : null;

        RefreshFill();
    }

    private void OnEnable()
    {
        RefreshFill();
    }

    private void Start()
    {
        RefreshFill();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ApplyPointerPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ApplyPointerPosition(eventData);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        OnPointerUp?.Invoke();
    }

    /// <summary>
    /// 외부에서 슬라이더 값을 코드로 설정할 때 사용한다.
    /// </summary>
    /// <param name="value">설정할 값</param>
    /// <param name="notify">OnValueChanged 통지 여부</param>
    public void SetValue(float value, bool notify = true)
    {
        _currentValue = Mathf.Clamp(value, _minValue, _maxValue);
        RefreshFill();
        OnVisualsUpdated?.Invoke(_currentValue);

        if (notify)
        {
            OnValueChanged?.Invoke(_currentValue);
        }
    }

    /// <summary>
    /// 외부에서 값을 코드로 직접 설정할 때 사용한다. OnValueChanged는 발생하지 않는다.
    /// </summary>
    public void SetValueWithoutNotify(float value)
    {
        SetValue(value, false);
    }

    // 포인터 위치에 맞춰 슬라이더 값을 산출하고 반영한다.
    private void ApplyPointerPosition(PointerEventData eventData)
    {
        if (_sliderBackground == null && transform is RectTransform rectTrans)
        {
            _sliderBackground = rectTrans;
        }

        if (_sliderBackground == null)
        {
            return;
        }

        // eventData에 수신된 카메라는 ScreenSpace - Camera / WorldSpace 대응을 위해 우선 활용
        Camera eventCam = eventData.pressEventCamera != null ? eventData.pressEventCamera : _uiCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _sliderBackground, eventData.position, eventCam, out Vector2 localPoint))
        {
            return;
        }

        Rect rect = _sliderBackground.rect;

        // 좌측 끝 0, 우측 끝 1 기준으로 비율 계산
        float ratio = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float newValue = Mathf.Clamp(Mathf.Lerp(_minValue, _maxValue, ratio), _minValue, _maxValue);

        _currentValue = newValue;
        RefreshFill();
        OnValueChanged?.Invoke(_currentValue);
        OnVisualsUpdated?.Invoke(_currentValue);
    }

    private void RefreshFill()
    {
        if (_fillImage == null)
        {
            return;
        }

        float ratio = Mathf.InverseLerp(_minValue, _maxValue, _currentValue);

        if (_fillImage.type == Image.Type.Filled)
        {
            _fillImage.fillAmount = ratio;
        }
        else
        {
            RectTransform fillRect = _fillImage.rectTransform;
            fillRect.anchorMin = new Vector2(0f, fillRect.anchorMin.y);
            fillRect.anchorMax = new Vector2(ratio, fillRect.anchorMax.y);
            fillRect.offsetMin = new Vector2(0f, fillRect.offsetMin.y);
            fillRect.offsetMax = new Vector2(0f, fillRect.offsetMax.y);
        }
    }
}
