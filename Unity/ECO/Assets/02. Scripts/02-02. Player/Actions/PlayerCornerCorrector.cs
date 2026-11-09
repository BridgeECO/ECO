using UnityEngine;
using VInspector;

public class PlayerCornerCorrector : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Collider2D _bodyCollider;

    [Foldout("Settings")]
    [SerializeField]
    private LayerMask _terrainLayer;

    [SerializeField]
    private float _correctionSpeed = 15f;

    // 몸통 윗변을 왼쪽·가운데·오른쪽 세 구역으로 나눈다. 가운데에 닿으면 천장에 정면으로 박은 것이라 멈추고,
    // 한쪽 구역에만 닿으면 모서리에 걸친 것이라 반대쪽으로 민다. 기본값은 몸통 폭 1.2 중 가운데 0.7이다.
    [SerializeField]
    [Range(0f, 1f)]
    private float _centerProbeWidthRatio = 0.7f / 1.2f;

    // OverlapBox는 두 콜라이더의 접촉 여유(defaultContactOffset의 2배)만큼 떨어진 지형까지 겹친 것으로 본다.
    // 검사 상자를 윗변 위로 띄우면 그만큼 천장을 일찍 감지하므로, 윗변 안쪽에 붙여 접촉 판정과 같은 거리를 쓴다.
    private const float PROBE_HEIGHT = 0.05f;

    // 닿았는지만 보면 되므로 결과 버퍼는 1칸이다. 매 물리 프레임 조회하므로 미리 할당한 배열을 재사용한다.
    private readonly Collider2D[] _probeResults = new Collider2D[1];
    private ContactFilter2D _terrainFilter;
    private PlayerMotor _motor;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _motor = GetComponentInParent<PlayerMotor>();
        _rigidbody = GetComponentInParent<Rigidbody2D>();
        InitTerrainFilter();
    }

    private void FixedUpdate()
    {
        UpdateCornerCorrection();
    }

    private void InitTerrainFilter()
    {
        _terrainFilter.SetLayerMask(_terrainLayer);
        // 프로젝트 설정에서 쿼리가 트리거도 잡으므로(queriesHitTriggers) 필터에서 명시적으로 뺀다.
        _terrainFilter.useTriggers = false;
    }

    private void UpdateCornerCorrection()
    {
        if (GetRisingSpeed() <= 0f)
        {
            return;
        }

        Bounds bodyBounds = _bodyCollider.bounds;
        float centerWidth = bodyBounds.size.x * _centerProbeWidthRatio;
        float sideWidth = (bodyBounds.size.x - centerWidth) * 0.5f;

        bool isCenterHitting = IsProbeHitting(bodyBounds.center.x, centerWidth, bodyBounds.max.y);
        if (isCenterHitting)
        {
            _motor.SetVelocityY(0f);
            return;
        }

        bool isLeftHitting = IsProbeHitting(bodyBounds.min.x + sideWidth * 0.5f, sideWidth, bodyBounds.max.y);
        bool isRightHitting = IsProbeHitting(bodyBounds.max.x - sideWidth * 0.5f, sideWidth, bodyBounds.max.y);
        if (isLeftHitting && !isRightHitting)
        {
            ApplyCorrection(1f);
        }
        if (isRightHitting && !isLeftHitting)
        {
            ApplyCorrection(-1f);
        }
    }

    // 점프로 오를 때뿐 아니라 움직이는 지형에 실려 오를 때도 머리가 천장에 부딪히므로, 타고 있는 지형이 실어 올리는 속도를 더한다.
    // 실려 오르다 한쪽 구역만 걸리면 점프 때처럼 옆으로 밀어 모서리를 빠져나가게 하고, 가운데 구역이 걸리면
    // 플레이어 스스로는 멈출 수 없으니 압사 판정(PlayerCrushDetector)이 끼임으로 처리한다.
    private float GetRisingSpeed()
    {
        return _motor.Velocity.y + _motor.ExternalVelocity.y;
    }

    private bool IsProbeHitting(float centerX, float width, float bodyTop)
    {
        Vector2 point = new Vector2(centerX, bodyTop - PROBE_HEIGHT * 0.5f);
        Vector2 size = new Vector2(width, PROBE_HEIGHT);
        return 0 < Physics2D.OverlapBox(point, size, 0f, _terrainFilter, _probeResults);
    }

    private void ApplyCorrection(float direction)
    {
        float pushAmount = _bodyCollider.bounds.extents.x * _correctionSpeed * Time.fixedDeltaTime;
        _rigidbody.position += Vector2.right * (direction * pushAmount);
    }
}
