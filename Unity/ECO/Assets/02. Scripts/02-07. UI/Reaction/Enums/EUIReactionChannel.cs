/// <summary>
/// 리액션이 건드리는 속성 갈래. 충돌 중재의 단위가 된다.
///
/// 같은 (대상 오브젝트, 채널)을 노리는 리액션끼리만 경합하고, 다른 채널은 전부 동시에 재생된다.
/// 예: Hover가 Scale을, Selected가 Alpha를 건드리면 둘 다 살아 있다.
/// </summary>
public enum EUIReactionChannel
{
    Position = 0,
    Rotation = 1,
    Scale = 2,
    Alpha = 3,
    Color = 4,

    /// <summary>단일 교체·시퀀스 재생·기존 애니메이터 연동이 공유한다. 겹치면 매 프레임 깜빡인다.</summary>
    Sprite = 5,

    Material = 6,
    Active = 7,
    ComponentEnable = 8,

    /// <summary>사운드는 되돌릴 상태가 없어 경합하지 않는다. 분류용으로만 둔다.</summary>
    Sound = 9,
}
