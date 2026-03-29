using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField]
    private BoxCollider2D _bodyCollider;
    [SerializeField]
    private CircleCollider2D _interactionCollider;
    [SerializeField]
    private BoxCollider2D _feetCollider;
    [SerializeField]
    private EdgeCollider2D _leftSlipCollider;
    [SerializeField]
    private EdgeCollider2D _rightSlipCollider;

    [Header("Layer Masks")]
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private LayerMask _platformLayer;
    [SerializeField]
    private LayerMask _interactionLayer;

    public bool IsGrounded { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsBodyTouching { get; private set; }
    public float WallDirection { get; private set; }

    private void Update()
    {
        IsGrounded = _feetCollider.IsTouchingLayers(_groundLayer | _platformLayer);

        IsBodyTouching = _bodyCollider.IsTouchingLayers(_groundLayer);

        IsSliding = _leftSlipCollider.IsTouchingLayers(_groundLayer) ||
                    _rightSlipCollider.IsTouchingLayers(_groundLayer);

        if (IsBodyTouching)
        {
            WallDirection = transform.localScale.x > 0 ? 1f : -1f;
        }
        else
        {
            WallDirection = 0f;
        }
    }

    public Collider2D GetInteractable()
    {
        return Physics2D.OverlapCircle(_interactionCollider.bounds.center, _interactionCollider.radius, _interactionLayer);
    }
}