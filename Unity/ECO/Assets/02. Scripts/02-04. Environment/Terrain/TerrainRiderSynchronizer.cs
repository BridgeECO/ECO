using System.Collections.Generic;
using UnityEngine;

public class TerrainRiderSynchronizer : MonoBehaviour
{
    private HashSet<Transform> _riders = new HashSet<Transform>();
    private Rigidbody2D _rb2D;
    private Vector2 _lastPosition;

    private void Awake()
    {
        _rb2D = GetComponent<Rigidbody2D>();
        _lastPosition = _rb2D.position;
    }

    private void FixedUpdate()
    {
        if (_riders.Count == 0)
        {
            _lastPosition = _rb2D.position;
            return;
        }

        Vector2 currentPosition = _rb2D.position;
        Vector2 delta = currentPosition - _lastPosition;

        if (delta.sqrMagnitude > 0.000001f)
        {
            foreach (var rider in _riders)
            {
                if (rider != null)
                {
                    rider.position += (Vector3)delta;
                }
            }
        }

        _lastPosition = currentPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(nameof(ETags.Player)))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    _riders.Add(collision.transform);
                    break;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(nameof(ETags.Player)))
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

            if (isOnTop)
            {
                _riders.Add(collision.transform);
            }
            else
            {
                _riders.Remove(collision.transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(nameof(ETags.Player)))
        {
            _riders.Remove(collision.transform);
        }
    }
}
