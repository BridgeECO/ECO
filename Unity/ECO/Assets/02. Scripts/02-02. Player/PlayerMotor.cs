using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public Vector2 Velocity { get; private set; }
    private PlayerStateMachine _stateMachine;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _stateMachine = GetComponent<PlayerStateMachine>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        Velocity = newVelocity;
    }

    public void SetVelocityX(float x)
    {
        Velocity = new Vector2(x, Velocity.y);
    }

    public void SetVelocityY(float y)
    {
        Velocity = new Vector2(Velocity.x, y);
    }

    public void AddVelocity(Vector2 addedVelocity)
    {
        Velocity += addedVelocity;
    }

    public void Teleport(Vector2 position)
    {
        _rigidbody.position = position;
        Velocity = Vector2.zero;
        _rigidbody.linearVelocity = Vector2.zero;
        _stateMachine.InitState();
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = Velocity;
    }
}