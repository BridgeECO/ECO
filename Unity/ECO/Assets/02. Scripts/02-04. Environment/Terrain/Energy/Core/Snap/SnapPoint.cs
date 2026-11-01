using UnityEngine;
using VInspector;

/// <summary>
/// 이동형 코어가 꺼져 지형이 멈출 때, 가까이 멈춘 지형을 끌어와 안착시키는 지점.
/// 코어의 스냅포인트 목록에 등록하고, 그 코어에 속한 지형의 웨이포인트 경로 위에 둔다.
/// </summary>
public class SnapPoint : MonoBehaviour
{
    // 레벨 디자이너가 경로 위에 손으로 놓으므로 이 정도 어긋남까지는 경로 위로 본다. 지형은 경로 위의 가장 가까운 점에 안착한다.
    public const float PATH_TOLERANCE = 0.25f;

    private const float MARKER_RADIUS = 0.2f;

    [Foldout("Settings")]
    [Tooltip("지형이 이 지점에서 경로를 따라 이 거리 안에 멈추면 이 지점으로 안착합니다. 직선거리가 아니라 웨이포인트 경로를 따라 잰 거리입니다.")]
    [Min(0f)]
    [SerializeField]
    private float _snapDistance = 1f;

    [Tooltip("안착할 때 평소 이동 속도에 곱하는 비율입니다. 기획상 평소보다 천천히 안착합니다.")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float _settleSpeedRatio = 0.5f;

    public float SnapDistance => _snapDistance;
    public float SettleSpeedRatio => _settleSpeedRatio;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, MARKER_RADIUS);
#endif
    }

    // 경로를 따라 잰 정확한 범위는 코어를 선택하면 경로 위에 그려진다. 여기서는 그 범위를 넘지 않는 직선 반경만 보여 준다.
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.magenta;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, _snapDistance);
#endif
    }
}
