using UnityEngine;

public class PlayerColliderRelay : MonoBehaviour
{
    private PlayerLife _playerLife;

    private void Awake()
    {
        _playerLife = GetComponentInParent<PlayerLife>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _playerLife.RelayTriggerEnter2D(collision.collider);
    }
}
