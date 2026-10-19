using UnityEngine;

public class PlayerSlip
{
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    public PlayerSlip(PlayerSensor sensor, PlayerMotor motor, PlayerDataSO data)
    {
        _sensor = sensor;
        _motor = motor;
        _data = data;
    }

    public void Handle()
    {
        if (!_sensor.IsSliding || _sensor.IsOnGround || 0f < _motor.Velocity.y)
        {
            return;
        }

        float velocityX = _data.SlipHorizontalSpeed * GetSlipDirection();
        _motor.SetVelocityX(velocityX);
        _motor.AddVelocity(Vector2.down * _data.SlipDownSpeed * Time.deltaTime);
    }

    private float GetSlipDirection()
    {
        return (_sensor.IsLeftSlipColliderTouching && _motor.IsForward) ||
               (_sensor.IsRightSlipColliderTouching && !_motor.IsForward)
               ? 1f : -1f;
    }
}
