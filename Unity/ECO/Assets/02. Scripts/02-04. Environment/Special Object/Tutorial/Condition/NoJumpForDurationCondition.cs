using System;
using UnityEngine;

/// <summary>
/// 인식 범위 안에서 지정 시간 동안 점프 입력이 한 번도 없으면 만족한다.
/// 점프하는 순간 다시 불만족으로 돌아가므로, 안내가 표시된 뒤의 이 전이가
/// 곧 "플레이어가 조작을 익혔다"는 신호가 되어 안내를 거둘 근거가 된다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. [MovedFrom] 없이 리네임하지 말 것.
/// </summary>
[Serializable]
public class NoJumpForDurationCondition : TutorialConditionBase
{
    [SerializeField]
    private float _requiredSeconds = 2f;

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
            _playerInput.OnJumpPressed += HandleJumpPressed;
        }
    }

    public override void Unbind()
    {
        if (_playerInput != null)
        {
            _playerInput.OnJumpPressed -= HandleJumpPressed;
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
        // 리스폰·팝업·씬 전환 중에는 점프 입력 자체가 발행되지 않으므로,
        // 이때 타이머를 돌리면 "점프를 못 했다"고 오판하게 된다.
        if (_isSatisfied || !isPlayerInRange || InputHandler.IsInputBlocked)
        {
            return;
        }

        _elapsedSeconds += deltaTime;
        if (_requiredSeconds <= _elapsedSeconds)
        {
            _isSatisfied = true;
        }
    }

    private void HandleJumpPressed()
    {
        _elapsedSeconds = 0f;
        _isSatisfied = false;
    }
}
