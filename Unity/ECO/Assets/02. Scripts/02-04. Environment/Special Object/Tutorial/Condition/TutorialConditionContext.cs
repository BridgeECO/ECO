using UnityEngine;

/// <summary>
/// 튜토리얼 조건이 판정에 사용하는 플레이어 참조 묶음.
/// 플레이어에 대한 전역 접근점이 없고 Find 계열 탐색이 금지되어 있으므로,
/// 인식 범위에 들어온 콜라이더에서 역으로 수집해 조건에 주입한다.
/// </summary>
public readonly struct TutorialConditionContext
{
    public PlayerInput Input { get; }
    public PlayerStateMachine StateMachine { get; }
    public PlayerSensor Sensor { get; }
    public Transform PlayerTransform { get; }

    public TutorialConditionContext(PlayerInput input, PlayerStateMachine stateMachine, PlayerSensor sensor, Transform playerTransform)
    {
        Input = input;
        StateMachine = stateMachine;
        Sensor = sensor;
        PlayerTransform = playerTransform;
    }
}
