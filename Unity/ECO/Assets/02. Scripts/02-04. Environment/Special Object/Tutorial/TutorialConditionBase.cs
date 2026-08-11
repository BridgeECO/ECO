using System;

/// <summary>
/// 튜토리얼 오브젝트의 발동 조건. 기획 의도에 따라 조건이 계속 새로 생기므로,
/// 파생 클래스 파일 하나를 추가하는 것만으로 조건이 늘어나고 기존 코드는 수정하지 않는다.
///
/// 이 계층은 [SerializeReference]로 씬/프리팹에 직렬화된다. 클래스명이나 네임스페이스가
/// 바뀌면 저장된 참조가 끊기므로, 리네임 시 반드시 [MovedFrom] 어트리뷰트를 남겨야 한다.
/// </summary>
[Serializable]
public abstract class TutorialConditionBase
{
    public abstract bool IsSatisfied { get; }

    /// <summary>플레이어 참조가 확보된 시점. 입력 이벤트 구독은 여기서 수행한다.</summary>
    public virtual void Bind(TutorialConditionContext context) { }

    /// <summary>구독을 해제한다. Bind 없이 호출되어도 안전해야 한다.</summary>
    public virtual void Unbind() { }

    /// <summary>누적된 감시 상태(타이머 등)를 초기화한다.</summary>
    public virtual void ResetCondition() { }

    /// <summary>시간 누적이 필요한 조건만 구현한다. 단순 조회형 조건은 IsSatisfied만으로 충분하다.</summary>
    public virtual void Tick(bool isPlayerInRange, float deltaTime) { }
}
