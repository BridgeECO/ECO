using UnityEngine;
using VInspector;

public class PlayerCornerCorrector : MonoBehaviour
{
    [Foldout("Head Colliders")]
    [SerializeField]
    private Collider2D _headLeft;

    [SerializeField]
    private Collider2D _headCenter;

    [SerializeField]
    private Collider2D _headRight;

    [Foldout("Body Reference")]
    [SerializeField]
    private Collider2D _bodyCollider;

    [Foldout("Settings")]
    [SerializeField]
    private LayerMask _terrainLayer;

    [SerializeField]
    private float _correctionSpeed = 15f;

    private PlayerMotor _motor;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _motor = GetComponentInParent<PlayerMotor>();
        _rigidbody = GetComponentInParent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        UpdateCornerCorrection();
    }

    private void UpdateCornerCorrection()
    {
        if (_motor.Velocity.y <= 0f)
        {
            return;
        }

        bool isCenterHitting = _headCenter.IsTouchingLayers(_terrainLayer);
        if (isCenterHitting)
        {
            return;
        }

        bool isLeftHitting = _headLeft.IsTouchingLayers(_terrainLayer);
        bool isRightHitting = _headRight.IsTouchingLayers(_terrainLayer);
        if (isLeftHitting && !isRightHitting)
        {
            ApplyCorrection(1f);
        }
        if (isRightHitting && !isLeftHitting)
        {
            ApplyCorrection(-1f);
        }
    }

    private void ApplyCorrection(float direction)
    {
        float pushAmount = _bodyCollider.bounds.extents.x * _correctionSpeed * Time.fixedDeltaTime;
        _rigidbody.position += Vector2.right * (direction * pushAmount);
    }
}
