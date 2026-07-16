using UnityEngine;

/// <summary>
/// 월드 좌표를 뷰포트 좌표로 변환하여 화면 밖 거리에 따른 볼륨 감쇠 배율을 계산하는 정적 유틸.
/// SoundEmitter와 WorldSfxPool이 동일한 감쇠 로직을 공유하기 위해 분리되었다.
/// </summary>
public static class ViewportAttenuator
{
    /// <summary>
    /// 월드 위치를 기준으로 뷰포트 거리 감쇠 배율(0~1)을 반환한다.
    /// 화면 안이면 1, falloffRange 이상 벗어나면 0.
    /// </summary>
    public static float Calculate(Vector3 worldPos, Camera cam, float falloffRange, AnimationCurve falloffCurve)
    {
        if (cam == null)
        {
            return 1f;
        }

        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        if (vp.z < 0f)
        {
            return falloffCurve.Evaluate(1f);
        }

        float dx = Mathf.Max(0f, -vp.x, vp.x - 1f);
        float dy = Mathf.Max(0f, -vp.y, vp.y - 1f);
        float outOfViewDist = Mathf.Max(dx, dy);

        float normalizedDist = Mathf.Clamp01(outOfViewDist / falloffRange);
        return falloffCurve.Evaluate(normalizedDist);
    }
}
