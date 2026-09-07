using System;
using UnityEngine;

/// <summary>
/// 지정한 스위치를 기준 시간만큼 붙잡지 못하고 놓은 횟수가 지정 횟수에 도달하면 만족한다.
/// 한 번이라도 충분히 오래 붙잡는 데 성공하면 불만족으로 돌아가므로, 짝이 될 해제 조건이 따로 필요 없다.
///
/// 관찰 시간 안의 실패를 보는 TC_SwitchNotHeld와 달리 실패 횟수를 센다. 스위치를 짧게 눌러
/// 목적지에 닿지 못하는 일이 몇 번 반복되었는지가 기준일 때 쓴다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. 리네임할 때는 [MovedFrom]에 이전 이름을 남긴다.
/// </summary>
[Serializable]
public class TC_RepeatedSwitchHoldFail : TutorialConditionBase
{
    [SerializeField]
    private EnergySwitch _targetSwitch;

    [Tooltip("이만큼 붙잡았다가 놓으면 성공으로 본다.")]
    [SerializeField]
    private float _requiredHoldSeconds = 2f;

    [SerializeField]
    private int _requiredFailureCount = 2;

    private float _heldSeconds;
    private int _failureCount;
    private bool _isSwitchOn;
    private bool _isWatching;
    private bool _hasSucceeded;

    public override bool IsSatisfied => !_hasSucceeded && _requiredFailureCount <= _failureCount;

    // ResetCondition은 일부러 구현하지 않는다. 실패한 플레이어는 스위치로 돌아오기까지 범위를
    // 벗어나는데, 그때마다 호출되는 여기서 되돌리면 정작 세어야 할 실패가 매번 지워진다.

    // 인식 범위로 막지 않는 것도 같은 이유다. 실패는 플레이어가 스위치에서 떨어져 있는 동안 확정된다.
    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        // 대상 미지정은 판정 불가로 두어 할당 누락이 곧바로 드러나게 한다.
        // 입력이 막힌 동안의 꺼짐은 플레이어가 놓은 것이 아니므로 함께 감시를 끊는다.
        if (_targetSwitch == null || InputHandler.IsInputBlocked)
        {
            _isWatching = false;
            return;
        }

        // 감시가 멈춘 사이의 상태 변화는 조작으로 볼 수 없다. 재개 첫 프레임은 값만 맞춘다.
        if (!_isWatching)
        {
            _isWatching = true;
            _isSwitchOn = _targetSwitch.IsOn;
            _heldSeconds = 0f;
            return;
        }

        if (_targetSwitch.IsOn)
        {
            _isSwitchOn = true;
            _heldSeconds += deltaTime;
            return;
        }

        if (!_isSwitchOn)
        {
            return;
        }

        _isSwitchOn = false;
        CountHoldResult();
    }

    private void CountHoldResult()
    {
        if (_requiredHoldSeconds <= _heldSeconds)
        {
            _hasSucceeded = true;
        }
        else
        {
            _failureCount++;
        }

        _heldSeconds = 0f;
    }
}
