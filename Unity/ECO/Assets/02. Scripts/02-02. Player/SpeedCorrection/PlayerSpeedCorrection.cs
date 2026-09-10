/// <summary>
/// 외부 요인 하나가 플레이어에게 거는 추가 속도 보정. 소유자는 PlayerSpeedCorrectionRegistry다.
/// </summary>
public struct PlayerSpeedCorrection
{
    public int SourceId;
    public float Magnitude;
    public ESpeedCorrectionDirection Direction;
}
