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

    private Collider2D[] _overlapResults = new Collider2D[4];
    private Collider2D _lastPlatformCollider;
    private PlatformEffector2D _cachedEffector;
    private ContactFilter2D _contactFilter;


    public bool IsOnGround { get; private set; }
    public bool IsOnPlatform { get; private set; }
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
        UpdateOnGroundSensor(touchingPlatform);
        UpdateOnPlatformSensor(touchingPlatform);
        UpdateBodySensor();
        UpdateWallSensor();
        UpdateSlipSensor();
    }

    private void UpdateOnGroundSensor(bool touchingPlatform)
    {
        // 지형 모서리가 둥글어서 생기는 미세한 틈새(V자 홈)에서도 접지 판정이 유지되도록 검사 영역을 아래로 약간 확장합니다.
        Vector2 checkSize = _feetCollider.bounds.size;
        checkSize.y += 0.1f;
        Vector3 checkCenter = _feetCollider.bounds.center;
        checkCenter.y -= 0.05f;

        bool touchingTerrain = CheckOverlap(checkCenter, checkSize, _terrainLayer);
        IsOnGround = touchingTerrain || touchingPlatform;
    }

    private void UpdateOnPlatformSensor(bool touchingPlatform)
    {
        if (!touchingPlatform)
        {
            ClearPlatformCache();
            return;
        }

        if (CheckOverlap(_feetCollider.bounds.center, _feetCollider.bounds.size, _platformLayer))
        {
            Collider2D hit = _overlapResults[0];
            if (!ReferenceEquals(hit, _lastPlatformCollider))
            {
                _lastPlatformCollider = hit;
                _cachedEffector = hit.GetComponent<PlatformEffector2D>();
            }

            CurrentPlatformEffector = _cachedEffector;
            IsOnPlatform = !ReferenceEquals(_cachedEffector, null);
        }
        else
        {
            ClearPlatformCache();
        }
    }

    private void UpdateBodySensor()
    {
        IsBodyTouching = CheckOverlap(_bodyCollider.bounds.center, _bodyCollider.bounds.size, _terrainLayer);
    }

    private void UpdateWallSensor()
    {
        IsWallTouching = CheckOverlap(_bodyCollider.bounds.center,
        (Vector2)_bodyCollider.bounds.size + new Vector2(WALL_CHECK_DISTANCE * 2f, 0f), _terrainLayer);
    }

    private void UpdateSlipSensor()
    {
        LayerMask slipLayers = _terrainLayer | _platformLayer;
        IsLeftSlipColliderTouching = CheckSlipOverlap(_leftSlipCollider, slipLayers);
        IsRightSlipColliderTouching = CheckSlipOverlap(_rightSlipCollider, slipLayers);
        IsSliding = IsLeftSlipColliderTouching || IsRightSlipColliderTouching;
    }

    private bool CheckSlipOverlap(EdgeCollider2D slipCollider, LayerMask slipLayers)
    {
        _contactFilter.layerMask = slipLayers;
        int count = slipCollider.Overlap(_contactFilter, _overlapResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = _overlapResults[i];
            if (col == null)
            {
                continue;
            }

            if (((1 << col.gameObject.layer) & _platformLayer) != 0)
            {
                if (col.TryGetComponent(out PlatformEffector2D effector) && effector.rotationalOffset == 180f)
                {
                    continue;
                }
            }
            return true;
        }
        return false;
    }

    private bool CheckOverlap(Vector2 center, Vector2 size, LayerMask layerMask)
    {
        _contactFilter.layerMask = layerMask;
        return 0 < Physics2D.OverlapBox(center, size, 0f, _contactFilter, _overlapResults);
    }

    private void ClearPlatformCache()
    {
        _lastPlatformCollider = null;
        _cachedEffector = null;
        CurrentPlatformEffector = null;
        IsOnPlatform = false;
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