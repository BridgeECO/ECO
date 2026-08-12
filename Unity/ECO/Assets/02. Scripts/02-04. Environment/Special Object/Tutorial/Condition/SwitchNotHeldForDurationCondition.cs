using System;
using UnityEngine;

/// <summary>
/// 인식 범위 안에서 지정한 스위치를 정해진 시간만큼 연속으로 켜둔 적이 없는 상태가
/// 관찰 시간을 넘겨 이어지면 만족한다.
///
/// 누르고 있어야 하는 버튼형과 밟고 있어야 하는 발판형은 조건 입장에서 모두
/// "켜진 상태를 유지한다"로 같다. 둘의 차이는 스위치 쪽 EInteractionType이 이미 담당하므로
/// 여기서는 한 조건으로 다룬다.
///
/// 유지에 성공하면 곧바로 불만족으로 돌아간다. 이 전이가 "퍼즐을 풀었다"는 신호가 되어
/// 안내를 거둘 근거가 된다.
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
        // 리스폰·팝업 중에는 스위치를 조작할 방법 자체가 없으므로, 이때 시간을 재면
        // "유지에 실패했다"고 오판하게 된다.
        // 대상 미지정은 판정 불가로 두어, 할당 누락 시 안내가 아예 뜨지 않아 곧바로 드러나게 한다.
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
