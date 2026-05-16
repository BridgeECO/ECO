using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField]
    private BoxCollider2D _bodyCollider;
    [SerializeField]
    private CircleCollider2D _interactionCollider;
    [SerializeField]
    private Collider2D _feetCollider;
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

    private const float WALL_CHECK_DISTANCE = 0.05f;
    private const float SLIP_CHECK_BOX_SIZE = 0.1f;

    private Collider2D[] _overlapResults = new Collider2D[1];
    private Collider2D _lastPlatformCollider;
    private PlatformEffector2D _cachedEffector;
    private ContactFilter2D _contactFilter;


    public bool IsGrounded { get; private set; }
    public bool HasPlatformEffector { get; private set; }
    public PlatformEffector2D CurrentPlatformEffector { get; private set; }
    public bool IsBodyTouching { get; private set; }
    public bool IsWallTouching { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsLeftSlipColliderTouching { get; private set; }
    public bool IsRightSlipColliderTouching { get; private set; }
    public float WallDirection { get; private set; }

    private void Start()
    {
        _contactFilter.useLayerMask = true;
    }

    private void Update()
    {
        UpdateSensors();
        HandleWallDirection();
    }

    private void UpdateSensors()
    {
        bool touchingPlatform = _feetCollider.IsTouchingLayers(_platformLayer);

        UpdateGroundSensor(touchingPlatform);
        UpdatePlatformEffectorSensor(touchingPlatform);
        UpdateBodyAndWallSensor();
        UpdateSlipSensor();
    }

    private void UpdateGroundSensor(bool touchingPlatform)
    {
        bool touchingTerrain = CheckOverlap(_feetCollider.bounds.center, _feetCollider.bounds.size, _terrainLayer);
        IsGrounded = touchingTerrain || touchingPlatform;
    }

    private void UpdatePlatformEffectorSensor(bool touchingPlatform)
    {
        if (!touchingPlatform)
        {
            ClearPlatformCache();
            return;
        }

        int platformCount = CheckOverlapCount(_feetCollider.bounds.center, _feetCollider.bounds.size, _platformLayer);
        if (0 < platformCount)
        {
            Collider2D hit = _overlapResults[0];
            if (!ReferenceEquals(hit, _lastPlatformCollider))
            {
                _lastPlatformCollider = hit;
                _cachedEffector = hit.GetComponent<PlatformEffector2D>();
            }

            CurrentPlatformEffector = _cachedEffector;
            HasPlatformEffector = !ReferenceEquals(_cachedEffector, null);
        }
        else
        {
            ClearPlatformCache();
        }
    }

    private void UpdateBodyAndWallSensor()
    {
        IsBodyTouching = CheckOverlap(_bodyCollider.bounds.center, _bodyCollider.bounds.size, _terrainLayer);
        IsWallTouching = CheckOverlap(_bodyCollider.bounds.center, (Vector2)_bodyCollider.bounds.size + new Vector2(WALL_CHECK_DISTANCE * 2f, 0f), _terrainLayer);
    }

    private void UpdateSlipSensor()
    {
        LayerMask slipLayers = _terrainLayer | _platformLayer;
        IsLeftSlipColliderTouching = _leftSlipCollider.IsTouchingLayers(slipLayers);
        IsRightSlipColliderTouching = _rightSlipCollider.IsTouchingLayers(slipLayers);
        IsSliding = IsLeftSlipColliderTouching || IsRightSlipColliderTouching;
    }

    private bool CheckOverlap(Vector2 center, Vector2 size, LayerMask layerMask)
    {
        return 0 < CheckOverlapCount(center, size, layerMask);
    }

    private int CheckOverlapCount(Vector2 center, Vector2 size, LayerMask layerMask)
    {
        _contactFilter.layerMask = layerMask;
        return Physics2D.OverlapBox(center, size, 0f, _contactFilter, _overlapResults);
    }

    private void ClearPlatformCache()
    {
        _lastPlatformCollider = null;
        _cachedEffector = null;
        CurrentPlatformEffector = null;
        HasPlatformEffector = false;
    }

    private void HandleWallDirection()
    {
        if (!IsWallTouching)
        {
            WallDirection = 0f;
            return;
        }
        bool isWallRight = Physics2D.Raycast
            (_bodyCollider.bounds.center, Vector2.right, _bodyCollider.bounds.extents.x + WALL_CHECK_DISTANCE, _terrainLayer);
        WallDirection = (isWallRight) ? 1f : -1f;
    }
}