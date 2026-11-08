using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField]
    private BoxCollider2D _bodyCollider;
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

    private const float WALL_CHECK_DISTANCE = 0.05f;
    private const float SLIP_CHECK_BOX_SIZE = 0.1f;

    // 벽과 몸통 접촉은 몸통 아래쪽(머리를 합치기 전 몸통 높이 1.1 / 2.1)만 본다. 머리 높이까지 보면 벽 아래 끝을 지날 때
    // 머리만 걸친 채 벽타기가 이어지고, 벽 방향 레이가 빗나가는 높이와 판정이 끝나는 높이가 어긋나 좌우 결과가 달라진다.
    private const float WALL_CHECK_HEIGHT_RATIO = 1.1f / 2.1f;

    private Collider2D[] _overlapResults = new Collider2D[4];
    private Collider2D _lastPlatformCollider;
    private PlatformEffector2D _cachedEffector;
    private ContactFilter2D _contactFilter;
    private PlayerGroundChecker _groundChecker;


    public bool IsOnGround { get; private set; }
    public bool IsOnPlatform { get; private set; }
    public PlatformEffector2D CurrentPlatformEffector { get; private set; }
    public bool IsBodyTouching { get; private set; }
    public bool IsWallTouching { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsLeftSlipColliderTouching { get; private set; }
    public bool IsRightSlipColliderTouching { get; private set; }
    public float WallDirection { get; private set; }
    public bool IsActuallyTouching => _feetCollider.IsTouchingLayers(_terrainLayer) || _feetCollider.IsTouchingLayers(_platformLayer);

    private void Awake()
    {
        _groundChecker = new PlayerGroundChecker(_feetCollider, _terrainLayer, _platformLayer);
    }

    private void Start()
    {
        _contactFilter.useLayerMask = true;
    }

    private void Update()
    {
        UpdateSensors();
        HandleWallDirection();
    }

    public bool TryGetPlatformLiftHeight(out float liftHeight)
    {
        return _groundChecker.TryGetPlatformLiftHeight(out liftHeight);
    }

    private void UpdateSensors()
    {
        bool touchingPlatform = _feetCollider.IsTouchingLayers(_platformLayer);
        IsOnGround = _groundChecker.CheckOnGround();
        UpdateOnPlatformSensor(touchingPlatform);
        UpdateBodySensor();
        UpdateWallSensor();
        UpdateSlipSensor();
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
        Bounds wallCheckBounds = GetWallCheckBounds();
        IsBodyTouching = CheckOverlap(wallCheckBounds.center, wallCheckBounds.size, _terrainLayer);
    }

    private void UpdateWallSensor()
    {
        Bounds wallCheckBounds = GetWallCheckBounds();
        IsWallTouching = CheckOverlap(wallCheckBounds.center,
        (Vector2)wallCheckBounds.size + new Vector2(WALL_CHECK_DISTANCE * 2f, 0f), _terrainLayer);
    }

    private Bounds GetWallCheckBounds()
    {
        Bounds bodyBounds = _bodyCollider.bounds;
        float height = bodyBounds.size.y * WALL_CHECK_HEIGHT_RATIO;
        Vector3 center = new Vector3(bodyBounds.center.x, bodyBounds.min.y + height * 0.5f, bodyBounds.center.z);
        return new Bounds(center, new Vector3(bodyBounds.size.x, height, bodyBounds.size.z));
    }

    private void UpdateSlipSensor()
    {
        LayerMask slipLayers = _terrainLayer | _platformLayer;
        IsLeftSlipColliderTouching = CheckSlipOverlap(_leftSlipCollider, slipLayers);
        IsRightSlipColliderTouching = CheckSlipOverlap(_rightSlipCollider, slipLayers);
        // 양쪽 감지기가 모두 닿았다면 모서리가 아니라 평평한 면에 걸친 것이라 밀어낼 방향이 없다.
        IsSliding = IsLeftSlipColliderTouching != IsRightSlipColliderTouching;
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

            if (((1 << col.gameObject.layer) & _platformLayer) != 0 && PlayerUnderJump.IsPassingThrough(col))
            {
                continue;
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
        Bounds wallCheckBounds = GetWallCheckBounds();
        bool isWallRight = Physics2D.Raycast
            (wallCheckBounds.center, Vector2.right, wallCheckBounds.extents.x + WALL_CHECK_DISTANCE, _terrainLayer);
        WallDirection = (isWallRight) ? 1f : -1f;
    }
}