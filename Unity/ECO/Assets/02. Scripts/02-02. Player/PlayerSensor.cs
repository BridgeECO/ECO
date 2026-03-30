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

    public bool IsGrounded => Physics2D.OverlapBox(_feetCollider.bounds.center, _feetCollider.bounds.size, 0f, _groundLayer | _platformLayer);
    public bool IsBodyTouching => Physics2D.OverlapBox(_bodyCollider.bounds.center, _bodyCollider.bounds.size, 0f, _groundLayer);
    public bool IsSliding => _leftSlipCollider.IsTouchingLayers(_groundLayer | _platformLayer) || 
                            _rightSlipCollider.IsTouchingLayers(_groundLayer | _platformLayer);
    public float WallDirection { get; private set; }

    private void Update()
    {
        HandleWallDirection();
    }

    private void HandleWallDirection()
    {
        if (IsBodyTouching)
        {
            WallDirection = 0 < transform.localScale.x ? 1f : -1f;
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