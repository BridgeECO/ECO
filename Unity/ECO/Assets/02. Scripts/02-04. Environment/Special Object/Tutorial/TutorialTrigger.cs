using System.Collections.Generic;

/// <summary>
/// 튜토리얼의 발동 판정. 조건 묶음과 진행 상태를 쥐고 "지금 보여야 하는가"만 답한다.
/// 트리거 감지와 UI 출력은 소유자(TutorialObject)의 몫이다.
///
/// 인스펙터에 노출할 데이터가 없으므로 직렬화하지 않고, 소유자가 Awake에서 조건 리스트를
/// 넘겨 생성한다. 직렬화 필드로 두면 인스펙터 중첩만 깊어진다.
/// </summary>
public class TutorialTrigger
{
    private readonly TutorialConditionSet _conditionSet;
    private readonly TutorialProgressState _progressState = new TutorialProgressState();
    private ETutorialObjectState _currentState = ETutorialObjectState.Idle;

    public bool IsVisible => _currentState == ETutorialObjectState.Visible;

    public TutorialTrigger(IReadOnlyList<TutorialConditionBase> conditions)
    {
        _conditionSet = new TutorialConditionSet(conditions);
    }

    /// <summary>표시 여부가 이번 프레임에 바뀌었으면 true를 반환한다.</summary>
    public bool Tick(bool isPlayerInRange, float deltaTime)
    {
        if (_currentState == ETutorialObjectState.Completed)
        {
            return false;
        }

        // 표시 판정보다 먼저 갱신한다. 범위 밖에서 벌어지는 일을 감시하는 조건(RepeatedFallCondition 등)은
        // 범위를 벗어난 뒤 얼마나 지났는지를 알아야 자기 구간의 사건만 골라낼 수 있다.
        _conditionSet.Tick(isPlayerInRange, deltaTime);

        if (_currentState == ETutorialObjectState.Idle && !isPlayerInRange)
        {
            return false;
        }

        if (_currentState == ETutorialObjectState.Idle)
        {
            if (!_conditionSet.IsAllSatisfied)
            {
                return false;
            }

            _currentState = ETutorialObjectState.Visible;
            return true;
        }

        // 표시 중에는 조건이 깨지는 순간이 곧 조작을 익혔다는 신호다.
        if (_conditionSet.IsAllSatisfied)
        {
            return false;
        }

        _currentState = ETutorialObjectState.Completed;
        _progressState.MarkFired();

        // 발동이 끝나면 더 감시할 이유가 없다. 다만 범위 안이라면 리스폰으로 되살아날 때
        // 진입 이벤트가 다시 오지 않으므로 바인딩을 남겨 둔다.
        if (!isPlayerInRange)
        {
            Release();
        }

        return true;
    }

    /// <summary>인식 범위와 무관하게 유지되는 조건 감시를 시작한다. 소유자의 활성화 시점에 호출한다.</summary>
    public void Activate()
    {
        _conditionSet.Activate();
    }

    /// <summary>감시를 모두 걷고 표시 상태를 되돌린다. 소유자의 비활성화 시점에 호출한다.</summary>
    public void Deactivate()
    {
        Suspend();
        _conditionSet.Deactivate();
    }

    public void Bind(TutorialConditionContext context)
    {
        _conditionSet.Bind(context);
        _conditionSet.ResetConditions();
    }

    public void Release()
    {
        _conditionSet.Unbind();
        _conditionSet.ResetConditions();
    }

    public void SaveProgress()
    {
        _progressState.SaveFiredState();
    }

    /// <summary>
    /// 표시 중이던 상태를 처음으로 되돌린다. Visible로 남겨두면 다시 켜져 재진입할 때
    /// 초기화된 조건이 곧바로 "깨진 것"으로 읽혀 발동이 끝나버린다.
    /// </summary>
    private void Suspend()
    {
        if (_currentState == ETutorialObjectState.Visible)
        {
            _currentState = ETutorialObjectState.Idle;
        }

        Release();
    }

    public void ResetToSavedState(bool isPlayerInRange)
    {
        _progressState.ResetToSavedState();

        if (_progressState.IsFired)
        {
            _currentState = ETutorialObjectState.Completed;
            Release();
            return;
        }

        _currentState = ETutorialObjectState.Idle;

        // 표시 중이었다면 범위 밖에서도 참조를 붙들고 있었으므로 여기서 놓는다.
        // 아직 범위 안이면 바인딩을 유지한 채 감시만 처음부터 다시 시작한다.
        if (isPlayerInRange)
        {
            _conditionSet.ResetConditions();
            return;
        }

        Release();
    }
}
