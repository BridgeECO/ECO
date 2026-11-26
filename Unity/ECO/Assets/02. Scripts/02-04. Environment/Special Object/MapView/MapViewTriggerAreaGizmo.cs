using UnityEngine;
using VInspector;

/// <summary>
/// 맵 조망 트리거의 판정 상자를 선택하지 않아도 씬 뷰에 늘 반투명으로 그려 주는 에디터 표시 전용 컴포넌트.
/// 보이지 않는 트리거라 배치할 때 전망대 전체를 덮는지 눈으로 맞출 수 있게 한다. 런타임 동작은 없다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class MapViewTriggerAreaGizmo : MonoBehaviour
{
    // 채움 너머 발판과 배경이 비쳐 보이는 정도의 에디터 표시 값이다.
    private const float FILL_ALPHA = 0.2f;

    [Foldout("Hierarchy")]
    [SerializeField]
    [Tooltip("씬 뷰에 영역을 그릴 트리거 상자. 같은 오브젝트의 BoxCollider2D를 할당합니다.")]
    private BoxCollider2D _triggerArea;

    // 클래스와 필드까지 감싸면 빌드에서 프리팹의 이 컴포넌트가 Missing Script가 되므로 그리기 메서드만 에디터 전용으로 둔다.
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 컴포넌트를 막 붙여 할당하기 전에도 리페인트마다 오류가 나지 않게 한다.
        if (_triggerArea == null)
        {
            return;
        }

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 center = new Vector3(_triggerArea.offset.x, _triggerArea.offset.y, 0f);
        Vector3 size = new Vector3(_triggerArea.size.x, _triggerArea.size.y, 0f);

        Color fillColor = Color.cyan;
        fillColor.a = FILL_ALPHA;
        Gizmos.color = fillColor;
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);

        Gizmos.matrix = previousMatrix;
    }
#endif
}
