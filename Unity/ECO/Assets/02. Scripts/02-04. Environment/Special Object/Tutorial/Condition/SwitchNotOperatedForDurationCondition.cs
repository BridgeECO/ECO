using System;
using UnityEngine;

/// <summary>
/// 인식 범위 안에서 지정한 스위치를 건드리지 않는 상태가 지정 시간 이상 이어지면 만족한다.
///
/// "조작"은 스위치의 켜짐/꺼짐이 바뀐 시점으로 본다. OnInteract는 발판형에서 발행되지 않아
/// 상태 변화를 보는 쪽만이 스위치 종류를 가리지 않는다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. [MovedFrom] 없이 리네임하지 말 것.
/// </summary>
[Serializable]
public class SwitchNotOperatedForDurationCondition : TutorialConditionBase
{
    [SerializeField]
    private EnergySwitch _targetSwitch;

    [SerializeField]
    private float _requiredSeconds = 2f;

    private float _elapsedSeconds;
    private bool _isSatisfied;
    private bool _isSwitchOn;
    private bool _isWatching;

    public override bool IsSatisfied => _isSatisfied;

    public override void ResetCondition()
    {
        _elapsedSeconds = 0f;
        _isSatisfied = false;
        _isWatching = false;
    }

    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        // 입력이 막힌 동안에는 스위치를 조작할 방법이 없어, 시간을 재면 조작하지 않았다고 오판한다.
        // 대상 미지정은 판정 불가로 두어 할당 누락이 곧바로 드러나게 한다.
        if (!isPlayerInRange || InputHandler.IsInputBlocked || _targetSwitch == null)
        {
            _isWatching = false;
            return;
        }

        // 감시가 멈춘 사이의 상태 변화는 조작으로 볼 수 없다. 재개 첫 프레임은 값만 맞춘다.
        if (!_isWatching)
        {
            _isWatching = true;
            _isSwitchOn = _targetSwitch.IsOn;
            ResetTimer();
            return;
        }

        if (_isSwitchOn != _targetSwitch.IsOn)
        {
            _isSwitchOn = _targetSwitch.IsOn;
            ResetTimer();
            return;
        }

        _elapsedSeconds += deltaTime;
        if (_requiredSeconds <= _elapsedSeconds)
        {
            _isSatisfied = true;
        }
    }

    private void ResetTimer()
    {
        _elapsedSeconds = 0f;
        _isSatisfied = false;
    }
}
