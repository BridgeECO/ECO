using UnityEngine;
using VInspector;

public class PlayerMotor : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private PhysicsMaterial2D _frictionlessMaterial;

    public Vector2 Velocity { get; private set; }
    public Vector2 ExternalVelocity { get; set; }
    public bool IsForward { get; private set; }
    private PlayerStateMachine _stateMachine;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _stateMachine = GetComponent<PlayerStateMachine>();
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_frictionlessMaterial == null)
        {
            CreatePhysicsMaterial2D();
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = Velocity + ExternalVelocity;
    }

    private void CreatePhysicsMaterial2D()
    {
        _frictionlessMaterial = new PhysicsMaterial2D();
        _frictionlessMaterial.friction = 0f;
        _frictionlessMaterial.bounciness = 0f;
    }

    public void SetFriction(bool enabled)
    {
        _rigidbody.sharedMaterial = enabled ? null : _frictionlessMaterial;
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

    public void SetFlip(float xInput)
    {
        if (xInput == 0f)
        {
            return;
        }
        Quaternion targetRotation = (0f < xInput) ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        if (transform.rotation == targetRotation)
        {
            return;
        }
        transform.rotation = targetRotation;
        IsForward = (0f < xInput);
    }

    public void AddVelocity(Vector2 addedVelocity)
    {
        Velocity += addedVelocity;
    }

    public void Teleport(Vector2 position)
    {
        transform.position = position;
        _rigidbody.position = position;
        Velocity = Vector2.zero;
        _rigidbody.linearVelocity = Vector2.zero;
        _stateMachine.InitState();
    }
}