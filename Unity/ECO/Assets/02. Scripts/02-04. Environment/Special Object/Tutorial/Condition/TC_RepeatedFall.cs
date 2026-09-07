using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

/// <summary>
/// 이 오브젝트의 인식 범위에서 낙사한 횟수가 지정 횟수에 도달하면 만족한다.
///
/// 만족이 단조 증가라 스스로 깨지지 않으므로, 안내를 거둘 조건(TC_NoJump 등)과
/// 반드시 함께 써야 한다. 단독으로 두면 한 번 뜬 안내가 사라지지 않는다.
///
/// 클래스명 변경 시 직렬화된 참조가 끊긴다. 리네임할 때는 [MovedFrom]에 이전 이름을 남긴다.
/// </summary>
[Serializable]
// 이름을 바꾸기 전 저장된 SerializeReference 참조를 잇는다.
[MovedFrom(false, null, null, "RepeatedFallCondition")]
public class TC_RepeatedFall : TutorialConditionBase
{
    [SerializeField]
    private int _requiredFallCount = 2;

    // 플레이어는 떨어지며 범위를 먼저 벗어난 뒤에 즉사 지형에 닿는다. 낙사 순간에는 이미 범위 밖이라
    // "벗어난 지 이만큼 이내"로 판정해야 한다.
    [SerializeField]
    private float _fallGraceSeconds = 2f;

    private int _fallCount;
    private float _secondsSinceInRange;
    private bool _isListenerAdded;

    public override bool IsSatisfied => _requiredFallCount <= _fallCount;

    public override void Activate()
    {
        // 소유자는 EventManager의 로드 순서를 몰라 Activate를 두 번 부른다. 이미 감시 중인데도
        // 되감으면 그사이 쌓인 낙사가 지워지므로, 구독에 실패했던 경우에만 다시 시도한다.
        if (_isListenerAdded)
        {
            return;
        }

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

    // ResetCondition은 일부러 구현하지 않는다. 범위 이탈에서도 호출되는데 떨어지는 순간이 곧
    // 범위 이탈이라, 여기서 되돌리면 정작 세어야 할 낙사를 놓친다.

    public override void Tick(bool isPlayerInRange, float deltaTime)
    {
        if (isPlayerInRange)
        {
            _secondsSinceInRange = 0f;
            return;
        }

        // 유예를 넘긴 뒤로는 값만 무한히 커지므로 더 재지 않는다.
        if (_secondsSinceInRange <= _fallGraceSeconds)
        {
            _secondsSinceInRange += deltaTime;
        }
    }

    // 전역 이벤트라 모든 튜토리얼 오브젝트에 똑같이 전달된다. 자기 구간의 낙사만 골라내지 않으면
    // 한 곳에서 실패한 기록으로 다른 구간의 안내가 떠버린다.
    private void HandlePlayerInstantKilled()
    {
        if (_fallGraceSeconds < _secondsSinceInRange)
        {
            return;
        }

        _fallCount++;

        // 즉사 지형에 닿은 채 리스폰 연출이 진행되면 충돌이 재발생한다. 한 번의 접근당 한 번만 센다.
        ExpireGrace();
    }

    private void ExpireGrace()
    {
        _secondsSinceInRange = float.MaxValue;
    }
}
