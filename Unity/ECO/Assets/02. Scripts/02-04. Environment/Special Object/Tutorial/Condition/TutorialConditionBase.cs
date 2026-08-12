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

    /// <summary>
    /// 인식 범위와 무관하게, 오브젝트가 살아 있는 동안 유지할 감시를 시작한다. 전역 이벤트 구독이 여기에 해당한다.
    /// 플레이어가 범위를 벗어나며 벌어지는 사건(예: 플랫폼에서 떨어져 낙사)은 Bind~Unbind 구간 밖이라
    /// 이 시점에 걸어둔 감시로만 관측할 수 있다.
    ///
    /// 전역 이벤트는 맵에 배치된 모든 튜토리얼 오브젝트에 똑같이 전달된다. 구독하는 조건은 반드시
    /// 자기 인식 범위로 스코핑해, 다른 곳에서 벌어진 사건을 자기 것으로 세지 않도록 해야 한다.
    ///
    /// 매니저가 아직 없는 로드 순서를 위해 소유자가 두 번 호출할 수 있다. 중복 구독이 남지 않도록
    /// 첫 줄에서 Deactivate를 먼저 부르는 식으로 몇 번 호출되어도 같은 결과가 되게 구현한다.
    /// </summary>
    public virtual void Activate() { }

    /// <summary>감시를 중단한다. Activate 없이 호출되어도 안전해야 한다.</summary>
    public virtual void Deactivate() { }

    /// <summary>플레이어 참조가 확보된 시점. 입력 이벤트 구독은 여기서 수행한다.</summary>
    public virtual void Bind(TutorialConditionContext context) { }

    /// <summary>구독을 해제한다. Bind 없이 호출되어도 안전해야 한다.</summary>
    public virtual void Unbind() { }

    /// <summary>
    /// 누적된 감시 상태(타이머 등)를 초기화한다.
    /// 리스폰마다 호출되므로, 리스폰을 넘어 유지되어야 하는 기록은 여기서 지우지 않는다.
    /// </summary>
    public virtual void ResetCondition() { }

    /// <summary>시간 누적이 필요한 조건만 구현한다. 단순 조회형 조건은 IsSatisfied만으로 충분하다.</summary>
    public virtual void Tick(bool isPlayerInRange, float deltaTime) { }
}
