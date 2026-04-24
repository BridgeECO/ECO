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
    private LayerMask _terrainLayer;
    [SerializeField]
    private LayerMask _platformLayer;
    [SerializeField]
    private LayerMask _interactionLayer;

    public bool IsGrounded => Physics2D.OverlapBox(_feetCollider.bounds.center, _feetCollider.bounds.size, 0f, _terrainLayer | _platformLayer);
    public bool IsBodyTouching => Physics2D.OverlapBox(_bodyCollider.bounds.center, _bodyCollider.bounds.size, 0f, _terrainLayer);
    public bool IsSliding => IsLeftSliding || IsRightSliding;
    public bool IsLeftSliding => _leftSlipCollider.IsTouchingLayers(_terrainLayer | _platformLayer);
    public bool IsRightSliding => _rightSlipCollider.IsTouchingLayers(_terrainLayer | _platformLayer);
    public float WallDirection { get; private set; }

    private void Update()
    {
        HandleWallDirection();
    }

    private void HandleWallDirection()
    {
        if (!IsBodyTouching)
        {
            WallDirection = 0f;
            return;
        }
        bool isWallRight = Physics2D.Raycast
            (_bodyCollider.bounds.center, Vector2.right, _bodyCollider.bounds.extents.x + 0.1f, _terrainLayer);
        WallDirection = (isWallRight) ? 1f : -1f;
    }

    public Collider2D GetInteractable()
    {
        return Physics2D.OverlapCircle(_interactionCollider.bounds.center, _interactionCollider.radius, _interactionLayer);
    }
}