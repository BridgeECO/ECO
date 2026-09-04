using System;
using UnityEngine;

/// <summary>
/// 인식 범위 안에서 지정 시간 동안 좌우 이동 입력이 없으면 만족한다.
/// 이동하는 순간 불만족으로 돌아가므로, 그 전이가 곧 안내를 거둘 신호가 된다.
///
/// 점프나 대시는 조작으로 세지 않는다. 아무 입력이나 받아주는 TC_NoInput을 쓰면
/// 제자리 점프만으로 이동 안내가 사라져, 정작 이동을 익히지 못한 플레이어를 놓친다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. 리네임할 때는 [MovedFrom]에 이전 이름을 남긴다.
/// </summary>
[Serializable]
public class TC_NoMove : TutorialConditionBase
{
    [SerializeField]
    private float _requiredSeconds = 2f;

    private PlayerInput _playerInput;
    private float _elapsedSeconds;
    private bool _isSatisfied;

    public override bool IsSatisfied => _isSatisfied;

    public override void Bind(TutorialConditionContext context)
    {
        _playerInput = context.Input;
    }

    public override void Unbind()
    {
        _playerInput = null;
    }

    public override void ResetCondition()
    {
        _elapsedSeconds = 0f;
        _isSatisfied = false;
    }

    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        // 도입부 연출처럼 입력이 막힌 동안 시간을 재면 조작할 수 없던 시간까지 헤맨 것으로 오판한다.
        if (!isPlayerInRange || InputHandler.IsInputBlocked)
        {
            return;
        }

        // 이동은 PlayerInput이 이벤트를 노출하지 않아 폴링한다. (TC_NoInput과 같은 방식)
        if (_playerInput != null && !Mathf.Approximately(_playerInput.HorizontalInput, 0f))
        {
            _elapsedSeconds = 0f;
            _isSatisfied = false;
            return;
        }

        _elapsedSeconds += deltaTime;
        if (_requiredSeconds <= _elapsedSeconds)
        {
            _isSatisfied = true;
        }
    }
}
