using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class TerrainRiderSynchronizer : MonoBehaviour
{
    private PlayerMotor _rider;
    private bool _isExiting;
    private float _exitingTime;

    private const float RIDER_COYOTE_TIME = 0.15f;

    private IDisposable _exitDisposable;

    private void OnDisable()
    {
        _exitDisposable?.Dispose();
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
        bool isOnTop = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                isOnTop = true;
                break;
            }
        }
        if (!isOnTop || !collision.gameObject.CompareTag(nameof(ETags.Player)))
        {
            return;
        }

        _rider = collision.gameObject.GetComponentInParent<PlayerMotor>();
        _isExiting = false;
        _exitDisposable?.Dispose();
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
        _exitDisposable?.Dispose();
        SubscribeToRiderExit();
    }

    private void SubscribeToRiderExit()
    {
        float timeout = Time.time + RIDER_COYOTE_TIME;
        _exitDisposable = Observable.EveryUpdate().Subscribe(_ => MonitorRiderExit(timeout));
    }

    private void MonitorRiderExit(float timeout)
    {
        if (_rider == null)
        {
            _isExiting = false;
            _exitDisposable?.Dispose();
            return;
        }

        if (timeout <= Time.time || 0.1f < _rider.Velocity.y)
        {
            _rider.ExternalVelocity = Vector2.zero;
            _rider = null;
            _isExiting = false;
            _exitDisposable?.Dispose();
        }
    }
}
