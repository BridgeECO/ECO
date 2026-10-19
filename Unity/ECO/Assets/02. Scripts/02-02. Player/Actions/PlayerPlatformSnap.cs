public class PlayerPlatformSnap
{
    // 발판 선과 겹쳐 있던 동안의 접촉은 일방향 판정으로 꺼진 채 유지돼, 선에 딱 맞춰 올리면 발이 닿지 않아
    // 공중 상태가 풀리지 않는다. 물리 엔진의 접촉 여유(0.02)보다 조금 더 띄워 접촉을 끊고 위에서 다시 내려앉게 한다.
    private const float LIFT_CLEARANCE = 0.05f;

    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    public PlayerPlatformSnap(PlayerSensor sensor, PlayerMotor motor)
    {
        _sensor = sensor;
        _motor = motor;
    }

    // 발판 선이 발과 몸통 사이에 끼면 몸통만 발판에 얹혀 공중 상태가 풀리지 않으므로, 떨어지기 시작하는 순간
    // 발판 위로 올려 세운다. 쌓인 낙하 속도는 지워서 올라선 직후 얇은 발판 선을 뚫고 내려가지 않게 한다.
    public void Handle()
    {
        if (0f < _motor.Velocity.y)
        {
            return;
        }
        if (!_sensor.TryGetPlatformLiftHeight(out float liftHeight))
        {
            return;
        }

        _motor.Lift(liftHeight + LIFT_CLEARANCE);
        _motor.SetVelocityY(0f);
    }
}
