using UnityEngine;
using VInspector;

/// <summary>
/// 플레이어 상호작용 범위 원의 중심과 반지름을 몸통 콜라이더에 맞춘다.
/// 중심은 몸통 박스 가운데, 몸통 옆 변에서 원 둘레까지가 몸통 가로의 배율만큼이다.
/// </summary>
public class PlayerInteractRange : MonoBehaviour
{
    // 반지름 = 몸통 가로 x (몸통 가운데에서 옆 변까지 0.5 + 옆 변에서 원 둘레까지 배율)
    private const float BODY_HALF_WIDTH_RATIO = 0.5f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private BoxCollider2D _bodyCollider;

    [SerializeField]
    private CircleCollider2D _rangeCollider;

    [Foldout("Settings")]
    [SerializeField]
    private float _widthMultiplier;

    private void Awake()
    {
        InitRange();
    }

    // 루트와 몸통의 스케일이 섞여 있어 월드 값으로 재고, 원 Transform의 로컬 값으로 되돌린다.
    // Awake 시점에는 형제 콜라이더가 아직 물리에 등록되지 않아 bounds가 비어 있을 수 있으므로 Transform으로 환산한다.
    private void InitRange()
    {
        Transform bodyTransform = _bodyCollider.transform;
        Transform rangeTransform = _rangeCollider.transform;

        Vector3 worldCenter = bodyTransform.TransformPoint(_bodyCollider.offset);
        float bodyWorldWidth = _bodyCollider.size.x * Mathf.Abs(bodyTransform.lossyScale.x);

        Vector2 localCenter = rangeTransform.InverseTransformPoint(worldCenter);
        float worldRadius = bodyWorldWidth * (BODY_HALF_WIDTH_RATIO + _widthMultiplier);
        Vector3 lossyScale = rangeTransform.lossyScale;
        float worldScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));

        _rangeCollider.offset = localCenter;
        _rangeCollider.radius = worldRadius / worldScale;
    }
}
