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

    public void Update()
    {
        if (!_sensor.IsSliding || 0f < _motor.Velocity.y || _sensor.IsGrounded || _sensor.IsWallTouching)
        {
            _motor.SetFriction(true);
            return;
        }

        _motor.SetFriction(false);
        float velocityX = (_sensor.IsLeftSliding) ? 2f : -2f;
        _motor.SetVelocityX(velocityX);
        _motor.AddVelocity(Vector2.down * _data.SlipDownSpeed * Time.deltaTime);
    }

    public void Reset()
    {
        _motor.SetFriction(true);
    }
}
