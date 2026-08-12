using System;
using UnityEngine;

/// <summary>
/// 인식 범위 안에서 지정 시간 동안 아무 조작도 하지 않으면 만족한다.
/// 무엇이든 입력하는 순간 다시 불만족으로 돌아가므로, 그 전이가 곧 안내를 거둘 신호가 된다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. [MovedFrom] 없이 리네임하지 말 것.
/// </summary>
[Serializable]
public class NoInputForDurationCondition : TutorialConditionBase
{
    [SerializeField]
    private float _requiredSeconds = 3f;

    private PlayerInput _playerInput;
    private float _elapsedSeconds;
    private bool _isSatisfied;

    public override bool IsSatisfied => _isSatisfied;

    public override void Bind(TutorialConditionContext context)
    {
        Unbind();

        _playerInput = context.Input;
        if (_playerInput != null)
        {
            _playerInput.OnJumpPressed += HandleManipulated;
            _playerInput.OnDashPressed += HandleManipulated;
        }
    }

    public override void Unbind()
    {
        if (_playerInput != null)
        {
            _playerInput.OnJumpPressed -= HandleManipulated;
            _playerInput.OnDashPressed -= HandleManipulated;
            _playerInput = null;
        }
    }

    public override void ResetCondition()
    {
        _elapsedSeconds = 0f;
        _isSatisfied = false;
    }

    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        // 입력이 막힌 동안에는 입력 자체가 발행되지 않아, 타이머를 돌리면 가만히 있다고 오판한다.
        if (!isPlayerInRange || InputHandler.IsInputBlocked)
        {
            return;
        }

        if (IsManipulating())
        {
            HandleManipulated();
            return;
        }

        _elapsedSeconds += deltaTime;
        if (_requiredSeconds <= _elapsedSeconds)
        {
            _isSatisfied = true;
        }
    }

    // 이동은 PlayerInput이 이벤트를 노출하지 않아 폴링한다. 상호작용 키는 PlayerInput이 다루지 않아
    // InteractionBase 파생 클래스들과 같은 방식으로 직접 읽는다.
    private bool IsManipulating()
    {
        if (_playerInput != null
            && (!Mathf.Approximately(_playerInput.HorizontalInput, 0f) || !Mathf.Approximately(_playerInput.VerticalInput, 0f)))
        {
            return true;
        }

        return Input.GetKey(KeyCode.F);
    }

    private void HandleManipulated()
    {
        _elapsedSeconds = 0f;
        _isSatisfied = false;
    }
}
