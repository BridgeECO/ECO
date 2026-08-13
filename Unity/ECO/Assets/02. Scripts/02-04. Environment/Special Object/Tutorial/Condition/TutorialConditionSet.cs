using System.Collections.Generic;

/// <summary>
/// 조건 묶음에 대한 일괄 조작과 AND 판정. 모든 조건이 만족해야 발동한다.
/// 조건이 하나도 없으면 인식 범위 진입만으로 뜨는 간판형 튜토리얼이 된다.
///
/// 인스펙터에 노출되는 리스트는 TutorialObject가 직접 소유한다. 이 클래스가 리스트를
/// 직렬화하면 인스펙터 중첩만 한 겹 깊어지므로, 리스트를 넘겨받아 다루기만 한다.
/// </summary>
public class TutorialConditionSet
{
    private readonly IReadOnlyList<TutorialConditionBase> _conditions;

    public bool IsAllSatisfied
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                // 인스펙터에서 타입을 고르지 않은 빈 항목은 판정에서 제외한다.
                if (ReferenceEquals(_conditions[i], null))
                {
                    continue;
                }

                if (!_conditions[i].IsSatisfied)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private int Count => ReferenceEquals(_conditions, null) ? 0 : _conditions.Count;

    public TutorialConditionSet(IReadOnlyList<TutorialConditionBase> conditions)
    {
        _conditions = conditions;
    }

    public void Activate()
    {
        for (int i = 0; i < Count; i++)
        {
            if (!ReferenceEquals(_conditions[i], null))
            {
                _conditions[i].Activate();
            }
        }
    }

    public void Deactivate()
    {
        for (int i = 0; i < Count; i++)
        {
            if (!ReferenceEquals(_conditions[i], null))
            {
                _conditions[i].Deactivate();
            }
        }
    }

    public void Bind(TutorialConditionContext context)
    {
        for (int i = 0; i < Count; i++)
        {
            if (!ReferenceEquals(_conditions[i], null))
            {
                _conditions[i].Bind(context);
            }
        }
    }

    public void Unbind()
    {
        for (int i = 0; i < Count; i++)
        {
            if (!ReferenceEquals(_conditions[i], null))
            {
                _conditions[i].Unbind();
            }
        }
    }

    public void ResetConditions()
    {
        for (int i = 0; i < Count; i++)
        {
            if (!ReferenceEquals(_conditions[i], null))
            {
                _conditions[i].ResetCondition();
            }
        }
    }

    public void Tick(bool isPlayerInRange, float deltaTime)
    {
        for (int i = 0; i < Count; i++)
        {
            if (!ReferenceEquals(_conditions[i], null))
            {
                _conditions[i].Tick(isPlayerInRange, deltaTime);
            }
        }
    }
}
