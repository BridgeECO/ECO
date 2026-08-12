using System;
using UnityEngine;

/// <summary>
/// 이 오브젝트의 인식 범위에서 낙사한 횟수가 지정 횟수에 도달하면 만족한다.
/// 건너뛰기 구간에서 여러 번 떨어졌다는 것은 곧 이 플레이어에게 안내가 필요하다는 신호다.
///
/// 만족이 단조 증가라 스스로 깨지지 않는다. 안내를 거둘 신호를 주는 조건과 반드시 함께 써야 하며,
/// 의도한 조합은 이 조건(게이트) + NoJumpForDurationCondition(점프하면 익힌 것)이다.
/// 단독으로 두면 한 번 뜬 안내가 사라지지 않는다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. [MovedFrom] 없이 리네임하지 말 것.
/// </summary>
[Serializable]
public class RepeatedFallCondition : TutorialConditionBase
{
    [SerializeField]
    private int _requiredFallCount = 2;

    // 플레이어는 발판에서 떨어지며 인식 범위를 먼저 벗어난 뒤에야 즉사 지형에 닿는다.
    // 그래서 "낙사 순간 범위 안"이 아니라 "범위를 벗어난 지 이만큼 이내"로 판정해야 한다.
    [SerializeField]
    private float _fallGraceSeconds = 2f;

    private int _fallCount;
    private float _secondsSinceInRange;
    private bool _isListenerAdded;

    public override bool IsSatisfied => _requiredFallCount <= _fallCount;

    public override void Activate()
    {
        Deactivate();

        _fallCount = 0;
        ExpireGrace();

        if (EventManager.Instance == null)
        {
            return;
        }

        EventManager.Instance.AddEventListener(EEventType.PlayerInstantKilled, HandlePlayerInstantKilled);
        _isListenerAdded = true;
    }

    public override void Deactivate()
    {
        if (!_isListenerAdded)
        {
            return;
        }

        _isListenerAdded = false;
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerInstantKilled, HandlePlayerInstantKilled);
        }
    }

    // 낙하 기록은 리스폰을 넘어 유지되어야 한다. 리스폰마다 호출되는 이 지점에서 지우면
    // 매번 0으로 돌아가 "여러 번 실패"에 영원히 도달하지 못한다.
    public override void ResetCondition()
    {
        ExpireGrace();
    }

    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        if (isPlayerInRange)
        {
            _secondsSinceInRange = 0f;
            return;
        }

        // 유예 시간을 넘긴 뒤로는 더 잴 이유가 없다. 계속 더하면 값만 무한히 커진다.
        if (_secondsSinceInRange <= _fallGraceSeconds)
        {
            _secondsSinceInRange += deltaTime;
        }
    }

    // 즉사는 전역 이벤트라 맵에 배치된 모든 튜토리얼 오브젝트에 똑같이 전달된다.
    // 자기 구간의 낙사만 골라내지 않으면, 한 곳에서 실패한 기록으로
    // 아직 가보지도 않은 다른 구간의 안내가 진입 즉시 떠버린다.
    private void HandlePlayerInstantKilled()
    {
        if (_fallGraceSeconds < _secondsSinceInRange)
        {
            return;
        }

        _fallCount++;
    }

    private void ExpireGrace()
    {
        _secondsSinceInRange = float.MaxValue;
    }
}
