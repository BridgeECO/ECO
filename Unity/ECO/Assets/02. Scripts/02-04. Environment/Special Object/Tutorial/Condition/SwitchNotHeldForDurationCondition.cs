using System;
using UnityEngine;

/// <summary>
/// 인식 범위 안에서 지정한 스위치를 정해진 시간만큼 연속으로 켜둔 적이 없는 상태가
/// 관찰 시간을 넘겨 이어지면 만족한다. 유지에 성공하는 순간 불만족으로 돌아가고,
/// 그 전이가 곧 안내를 거둘 신호가 된다.
///
/// 버튼을 누르고 있는 것과 발판을 밟고 있는 것은 모두 "켜진 상태를 유지한다"로 같아 한 조건으로
/// 다룬다. 둘의 차이는 스위치 쪽 EInteractionType이 담당한다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. [MovedFrom] 없이 리네임하지 말 것.
/// </summary>
[Serializable]
public class SwitchNotHeldForDurationCondition : TutorialConditionBase
{
    [SerializeField]
    private EnergySwitch _targetSwitch;

    [SerializeField]
    private float _requiredHoldSeconds = 2f;

    [SerializeField]
    private float _observeSeconds = 3f;

    private float _heldSeconds;
    private float _observedSeconds;
    private bool _isSatisfied;

    public override bool IsSatisfied => _isSatisfied;

    public override void ResetCondition()
    {
        _heldSeconds = 0f;
        _observedSeconds = 0f;
        _isSatisfied = false;
    }

    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        // 입력이 막힌 동안에는 스위치를 조작할 방법이 없어, 시간을 재면 유지에 실패했다고 오판한다.
        // 대상 미지정은 판정 불가로 두어 할당 누락이 곧바로 드러나게 한다.
        if (!isPlayerInRange || InputHandler.IsInputBlocked || _targetSwitch == null)
        {
            return;
        }

        if (_targetSwitch.IsOn)
        {
            _heldSeconds += deltaTime;
        }
        else
        {
            _heldSeconds = 0f;
        }

        if (_requiredHoldSeconds <= _heldSeconds)
        {
            _observedSeconds = 0f;
            _isSatisfied = false;
            return;
        }

        _observedSeconds += deltaTime;
        if (_observeSeconds <= _observedSeconds)
        {
            _isSatisfied = true;
        }
    }
}
