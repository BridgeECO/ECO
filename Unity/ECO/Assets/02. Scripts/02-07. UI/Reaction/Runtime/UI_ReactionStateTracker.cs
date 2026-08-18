using System;

/// <summary>
/// 포인터·선택·상호작용 플래그로부터 지금 성립하는 UI 상태를 판정한다.
/// Selectable은 상태를 하나만 돌려줘 Hover와 Selected의 동시 성립을 표현하지 못한다.
/// </summary>
public class UI_ReactionStateTracker
{
    public Action OnStateChanged;

    private bool _isPointerInside;
    private bool _isPressed;
    private bool _isSelected;
    private bool _isInteractable = true;

    public bool IsInteractable => _isInteractable;

    public void SetPointerInside(bool isInside)
    {
        if (_isPointerInside == isInside)
        {
            return;
        }

        _isPointerInside = isInside;
        OnStateChanged?.Invoke();
    }

    public void SetPressed(bool isPressed)
    {
        if (_isPressed == isPressed)
        {
            return;
        }

        _isPressed = isPressed;
        OnStateChanged?.Invoke();
    }

    public void SetSelected(bool isSelected)
    {
        if (_isSelected == isSelected)
        {
            return;
        }

        _isSelected = isSelected;
        OnStateChanged?.Invoke();
    }

    public void SetInteractable(bool isInteractable)
    {
        if (_isInteractable == isInteractable)
        {
            return;
        }

        _isInteractable = isInteractable;
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// 상호작용이 막힌 동안에는 Disabled만 성립시킨다.
    /// interactable을 꺼도 레이캐스트는 살아 있어 포인터 이벤트가 계속 들어오기 때문에,
    /// 이 가드가 없으면 비활성 버튼이 hover 연출을 낸다.
    /// </summary>
    public bool IsActive(EUIReactionState state)
    {
        if (!_isInteractable)
        {
            return state == EUIReactionState.Disabled;
        }

        switch (state)
        {
            case EUIReactionState.Normal:
                return !_isPointerInside && !_isPressed && !_isSelected;

            case EUIReactionState.Hover:
                return _isPointerInside;

            case EUIReactionState.Pressed:
                return _isPressed;

            case EUIReactionState.Selected:
                return _isSelected;

            default:
                return false;
        }
    }

    /// <summary>
    /// 실제 입력 없이 한 상태만 켜 본다. 인스펙터 미리보기 전용이다.
    /// </summary>
    public void ApplyPreview(EUIReactionState state)
    {
        ResetState();

        switch (state)
        {
            case EUIReactionState.Hover:
                SetPointerInside(true);
                break;

            case EUIReactionState.Pressed:
                SetPressed(true);
                break;

            case EUIReactionState.Selected:
                SetSelected(true);
                break;

            case EUIReactionState.Disabled:
                SetInteractable(false);
                break;

            default:
                break;
        }
    }

    /// <summary>풀링된 UI가 재사용될 때 이전 상태가 남지 않도록 비운다.</summary>
    public void ResetState()
    {
        _isPointerInside = false;
        _isPressed = false;
        _isSelected = false;
        _isInteractable = true;
    }
}
