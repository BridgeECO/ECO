/// <summary>컷씬이 끝까지 못 간 이유. 사유마다 세계 상태를 어디까지 건드릴지가 다르다.</summary>
// 값이 직렬화될 일은 없지만, 0번은 가장 보수적인 쪽이어야 새 필드가 0으로 채워져도 안전하다.
public enum ECutsceneAbortReason
{
    /// <summary>사망·리스폰·방 리셋. 곧 Room이 세이브 시점으로 되돌리므로 리스만 놓는다.</summary>
    WorldWillReset = 0,

    /// <summary>비활성화·카메라 경합·씬 전환. 남은 스텝의 최종 상태를 찍어 어중간한 상태를 남기지 않는다.</summary>
    StopInPlace = 1,

    /// <summary>오브젝트가 파괴됐다. 대상이 이미 죽었을 수 있어 아무것도 쓰지 않는다.</summary>
    Destroyed = 2,
}
