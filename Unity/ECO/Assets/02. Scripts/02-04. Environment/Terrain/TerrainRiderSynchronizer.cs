using System.Collections.Generic;
using UnityEngine;

public class TerrainRiderSynchronizer : MonoBehaviour
{
    private PlayerMotor _rider;
    private bool _isExiting;
    private float _exitingTime;

    private const float RIDER_COYOTE_TIME = 0.15f;

    private void Update()
    {
        if (_rider != null && _isExiting)
        {
            if (Time.time >= _exitingTime || _rider.Velocity.y > 0.1f)
            {
                _rider.ExternalVelocity = Vector2.zero;
                _rider = null;
                _isExiting = false;
            }
        }
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (_rider != null)
        {
            _rider.ExternalVelocity = velocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(nameof(ETags.Player)))
        {
            return;
        }

        bool isOnTop = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                isOnTop = true;
                break;
            }
        }

        if (!isOnTop)
        {
            return;
        }
        PlayerMotor motor = collision.gameObject.GetComponentInParent<PlayerMotor>();
        if (motor == null)
        {
            return;
        }
        _rider = motor;
        _isExiting = false;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(nameof(ETags.Player)))
        {
            return;
        }

        PlayerMotor motor = collision.gameObject.GetComponentInParent<PlayerMotor>();
        if (motor == null || _rider != motor || _isExiting)
        {
            return;
        }
        _isExiting = true;
        _exitingTime = Time.time + RIDER_COYOTE_TIME;
    }
}
