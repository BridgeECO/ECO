using System;
using UnityEngine;

/// <summary>
/// 인식 범위 안에 지정 시간 머무는 동안 긴 점프를 한 번도 하지 않으면 만족한다.
/// 점프키를 기준 시간 이상 붙잡는 순간 불만족으로 돌아가고, 그 전이가 안내를 거둘 신호가 된다.
///
/// 점프에 실패해 떨어진 플레이어를 받는 바닥에 두는 것을 전제로 한다. 그 바닥에 들어온 것이
/// 곧 실패했다는 신호라 낙하를 따로 감지하지 않는다. 죽지 않고 떨어지는 지형이라
/// 낙사를 세는 TC_RepeatedFall로는 잡을 수 없다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. 리네임할 때는 [MovedFrom]에 이전 이름을 남긴다.
/// </summary>
[Serializable]
public class TC_NoLongJump : TutorialConditionBase
{
    [SerializeField]
    private float _requiredSeconds = 1f;

    [Tooltip("긴 점프로 인정할 점프키 유지 시간. PlayerDataSO의 MaxJumpHoldTime보다 조금 낮게 둔다.")]
    [SerializeField]
    private float _requiredHoldSeconds = 0.3f;

    private PlayerInput _playerInput;
    private float _elapsedSeconds;
    private float _heldSeconds;
    private bool _isJumpHeld;
    private bool _isSatisfied;

    public override bool IsSatisfied => _isSatisfied;

    public override void Bind(TutorialConditionContext context)
    {
        Unbind();

        _playerInput = context.Input;
        if (_playerInput == null)
        {
            return;
        }

        _playerInput.OnJumpPressed += HandleJumpPressed;
        _playerInput.OnJumpReleased += HandleJumpReleased;
    }

    public override void Unbind()
    {
        if (_playerInput == null)
        {
            return;
        }

        _playerInput.OnJumpPressed -= HandleJumpPressed;
        _playerInput.OnJumpReleased -= HandleJumpReleased;
        _playerInput = null;
    }

    public override void ResetCondition()
    {
        _elapsedSeconds = 0f;
        _heldSeconds = 0f;
        _isJumpHeld = false;
        _isSatisfied = false;
    }

    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        if (InputHandler.IsInputBlocked)
        {
            return;
        }

        // 홀드 누적을 범위 판정보다 먼저 한다. 안내가 떠 있는 동안에는 범위를 벗어나 뛰더라도
        // 성공을 잡아야 안내를 거둘 수 있다.
        if (TryConsumeLongJump(deltaTime))
        {
            return;
        }

        if (!isPlayerInRange || _isSatisfied)
        {
            return;
        }

        _elapsedSeconds += deltaTime;
        if (_requiredSeconds <= _elapsedSeconds)
        {
            _isSatisfied = true;
        }
    }

    /// <summary>긴 점프에 성공했으면 만족을 되돌리고 true를 반환한다.</summary>
    private bool TryConsumeLongJump(float deltaTime)
    {
        if (!_isJumpHeld)
        {
            return false;
        }

        _heldSeconds += deltaTime;
        if (_heldSeconds < _requiredHoldSeconds)
        {
            return false;
        }

        // 키를 떼기 전에 거둬야 플레이어가 자기 조작과 안내가 사라진 것을 바로 연결한다.
        _isJumpHeld = false;
        _elapsedSeconds = 0f;
        _isSatisfied = false;
        return true;
    }

    private void HandleJumpPressed()
    {
        _isJumpHeld = true;
        _heldSeconds = 0f;
    }

    private void HandleJumpReleased()
    {
        _isJumpHeld = false;
    }
}
