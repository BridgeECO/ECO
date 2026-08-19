#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

/// <summary>UI_Reactor를 잘못 배치했을 때 조용히 망가지는 대신 바로 알려 준다.</summary>
public static class UI_ReactorValidator
{
    public static void Validate(UI_Reactor reactor)
    {
        if (reactor == null)
        {
            return;
        }

        WarnMisplacedOnSelectableChild(reactor);
    }

    /// <summary>
    /// 부모에만 버튼이 있으면 잘못 붙은 것이다. ExecuteHierarchy가 포인터 이벤트를 Reactor에서
    /// 멈춰 버려 부모 버튼의 onClick이 아예 발화하지 않는다.
    /// </summary>
    private static void WarnMisplacedOnSelectableChild(UI_Reactor reactor)
    {
        if (reactor.GetComponent<Selectable>() != null)
        {
            return;
        }

        Selectable parentSelectable = reactor.GetComponentInParent<Selectable>(true);
        if (parentSelectable == null)
        {
            return;
        }

        Debug.LogError(
            $"[UI_Reactor] '{reactor.name}'이(가) 버튼 '{parentSelectable.name}'의 자식에 붙어 있습니다. " +
            "이대로 두면 부모 버튼의 클릭이 죽습니다. Reactor는 버튼과 같은 게임오브젝트에 두고, " +
            "자식 요소는 각 리액션의 대상 오브젝트로 지정하세요. " +
            "넓은 hover 영역이 필요하다면 UI_ReactionRelay를 쓰세요.",
            reactor);
    }
}
#endif
