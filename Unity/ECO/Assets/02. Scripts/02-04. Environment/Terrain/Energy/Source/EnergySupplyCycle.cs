/// <summary>
/// 주기형 에너지원의 공급과 차단을 정해진 시간마다 번갈아 바꾸는 타이머.
/// </summary>
public class EnergySupplyCycle
{
    private float _supplyDuration;
    private float _cutoffDuration;
    private float _remainingDuration;

    public bool IsSupplying { get; private set; }

    public void Init(float supplyDuration, float cutoffDuration)
    {
        _supplyDuration = supplyDuration;
        _cutoffDuration = cutoffDuration;
        Restart();
    }

    public void Restart()
    {
        IsSupplying = true;
        _remainingDuration = _supplyDuration;
    }

    // 공급과 차단이 바뀐 프레임에만 true를 돌려준다.
    public bool Tick(float deltaTime)
    {
        _remainingDuration -= deltaTime;
        if (0f < _remainingDuration)
        {
            return false;
        }

        IsSupplying = !IsSupplying;
        _remainingDuration = IsSupplying ? _supplyDuration : _cutoffDuration;
        return true;
    }
}
