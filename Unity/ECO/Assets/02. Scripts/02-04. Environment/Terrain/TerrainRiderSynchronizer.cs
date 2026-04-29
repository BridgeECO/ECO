using System.Collections.Generic;
using UnityEngine;

public class TerrainRiderSynchronizer : MonoBehaviour
{
    private List<PlayerMotor> _activeRiders = new List<PlayerMotor>();
    private List<PlayerMotor> _exitingRiders = new List<PlayerMotor>();
    private List<float> _exitingTimes = new List<float>();

    private const float RIDER_COYOTE_TIME = 0.15f;

    public void SetVelocity(Vector2 velocity)
    {
        for (int i = 0; i < _activeRiders.Count; i++)
        {
            if (_activeRiders[i] != null)
            {
                _activeRiders[i].ExternalVelocity = velocity;
            }
        }

        for (int i = 0; i < _exitingRiders.Count; i++)
        {
            if (_exitingRiders[i] != null)
            {
                _exitingRiders[i].ExternalVelocity = velocity;
            }
        }
    }

    private void Update()
    {
        for (int i = _exitingRiders.Count - 1; i >= 0; i--)
        {
            PlayerMotor rider = _exitingRiders[i];
            
            if (rider == null || Time.time >= _exitingTimes[i] || rider.Velocity.y > 0.1f)
            {
                if (rider != null)
                {
                    rider.ExternalVelocity = Vector2.zero;
                }
                _exitingRiders.RemoveAt(i);
                _exitingTimes.RemoveAt(i);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(nameof(ETags.Player)))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    PlayerMotor motor = collision.gameObject.GetComponentInParent<PlayerMotor>();
                    if (motor != null)
                    {
                        int exitIndex = _exitingRiders.IndexOf(motor);
                        if (exitIndex >= 0)
                        {
                            _exitingRiders.RemoveAt(exitIndex);
                            _exitingTimes.RemoveAt(exitIndex);
                        }

                        if (!_activeRiders.Contains(motor))
                        {
                            _activeRiders.Add(motor);
                        }
                    }
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(nameof(ETags.Player)))
        {
            PlayerMotor motor = collision.gameObject.GetComponentInParent<PlayerMotor>();
            if (motor != null)
            {
                if (_activeRiders.Remove(motor))
                {
                    if (!_exitingRiders.Contains(motor))
                    {
                        _exitingRiders.Add(motor);
                        _exitingTimes.Add(Time.time + RIDER_COYOTE_TIME);
                    }
                }
            }
        }
    }
}
