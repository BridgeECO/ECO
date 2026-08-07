using System.Threading;
using UnityEngine;

/// <summary>
/// State가 상태 전이와 공유 블랙보드에 접근하기 위한 컨텍스트.
/// State가 PlayerStateMachine 구체 타입을 직접 참조하지 않도록 격리한다.
/// (SM→State는 IPlayerState, State→SM은 이 인터페이스로 양쪽 모두 추상에 의존)
/// </summary>
public interface IPlayerFSMContext
{
    PlayerInput Input { get; }
    PlayerSensor Sensor { get; }
    PlayerMotor Motor { get; }
    Animator Animator { get; }
    Transform Transform { get; }
    CancellationToken DestroyToken { get; }

    float JumpBufferTimer { get; set; }
    float CoyoteTimer { get; set; }
    bool HasUsedHover { get; set; }
    float DashCooldownTimer { get; set; }
    float InputLockTimer { get; set; }
    float LastWallJumpDir { get; set; }

    void ChangeState(EPlayerState newState);
}
