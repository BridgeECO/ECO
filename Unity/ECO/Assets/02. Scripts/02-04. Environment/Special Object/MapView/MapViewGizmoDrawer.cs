#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// 맵 조망 오브젝트가 비출 화면 범위를 씬 뷰에 그리는 에디터 전용 헬퍼.
/// 기획자가 플레이하지 않고도 조망 지점과 Z값이 만들어 낼 화면 크기를 눈으로 확인할 수 있게 한다.
/// </summary>
public static class MapViewGizmoDrawer
{
    // 씬에 MainCamera가 없을 때 쓰는 기본값. 런타임 카메라 프리팹의 설정과 같은 값이다.
    private const float FALLBACK_ASPECT = 16f / 9f;
    private const float FALLBACK_FIELD_OF_VIEW = 60f;

    public static void DrawViewFrame(Vector3 objectPosition, Vector3 viewPosition, float cameraZ)
    {
        Camera mainCamera = Camera.main;
        float aspect = mainCamera != null ? mainCamera.aspect : FALLBACK_ASPECT;
        float fieldOfView = mainCamera != null ? mainCamera.fieldOfView : FALLBACK_FIELD_OF_VIEW;

        // 원근 카메라에서 거리 d만큼 떨어진 평면의 화면 세로 절반은 d * tan(FOV/2)이다.
        // 오브젝트가 놓인 평면을 기준으로 삼아 실제로 담기는 범위를 그린다.
        float distance = Mathf.Abs(cameraZ - objectPosition.z);
        float halfHeight = distance * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);

        Vector3 frameCenter = new Vector3(viewPosition.x, viewPosition.y, objectPosition.z);
        Vector3 frameSize = new Vector3(halfHeight * aspect * 2f, halfHeight * 2f, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(frameCenter, frameSize);
        Gizmos.DrawLine(objectPosition, frameCenter);
    }
}
#endif
