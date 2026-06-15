using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class BossChasingLine : MonoBehaviour
{
    [Foldout("Path Settings")]
    [SerializeField, Tooltip("경로의 시작점 (비워두면 보스의 현재 위치 사용)")]
    private Transform _startPoint;

    [SerializeField, Tooltip("곡선을 그리기 위한 경유지들 (Waypoint)")]
    private List<Transform> _waypoints = new List<Transform>();

    [SerializeField, Tooltip("경로의 최종 도착점")]
    private Transform _endPoint;

    [Tooltip("곡선의 부드러움 분할 해상도 (적정값: 10 ~ 20)")]
    [SerializeField] private int _curveResolution = 10;

    [Tooltip("스플라인 곡선 경로의 장력 및 휘어짐 강도 (적정값: 0.5 ~ 1.0)")]
    [SerializeField] private float _splineTension = 1f;

    private List<Vector3> _computedWaypoints = new List<Vector3>();

    private void Awake()
    {
        CalculatePath();
    }

    /// <summary>
    /// 계산이 완료된 스플라인 곡선 경로 좌표 리스트를 반환합니다.
    /// </summary>
    public List<Vector3> GetComputedPath()
    {
        if (_computedWaypoints.Count == 0)
        {
            CalculatePath();
        }
        return _computedWaypoints;
    }

    public void CalculatePath()
    {
        _computedWaypoints.Clear();

        List<Vector3> keyPoints = new List<Vector3>();
        if (_startPoint != null) keyPoints.Add(_startPoint.position);
        else keyPoints.Add(transform.position);

        foreach (var wp in _waypoints)
        {
            if (wp != null) keyPoints.Add(wp.position);
        }

        if (_endPoint != null) keyPoints.Add(_endPoint.position);

        if (keyPoints.Count < 2)
        {
            if (keyPoints.Count == 1) _computedWaypoints.Add(keyPoints[0]);
            return;
        }

        Vector3[] tangents = SplineUtility.CalculateTangents(keyPoints);

        _computedWaypoints.Add(keyPoints[0]);

        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            Vector3 p0 = keyPoints[i];
            Vector3 p1 = keyPoints[i + 1];
            float dist = Vector3.Distance(p0, p1);

            Vector3 m0 = tangents[i] * (dist * _splineTension);
            Vector3 m1 = tangents[i + 1] * (dist * _splineTension);

            int resolution = Mathf.Max(1, _curveResolution);

            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 point = SplineUtility.GetHermiteCurvePosition(t, p0, p1, m0, m1);
                _computedWaypoints.Add(point);
            }
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        List<Vector3> keyPoints = new List<Vector3>();
        if (_startPoint != null) keyPoints.Add(_startPoint.position);
        else keyPoints.Add(transform.position);

        foreach (var wp in _waypoints) if (wp != null) keyPoints.Add(wp.position);
        if (_endPoint != null) keyPoints.Add(_endPoint.position);

        if (keyPoints.Count < 2) return;

        Vector3[] tangents = SplineUtility.CalculateTangents(keyPoints);

        Gizmos.color = Color.cyan;

        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            Vector3 p0 = keyPoints[i];
            Vector3 p1 = keyPoints[i + 1];
            float dist = Vector3.Distance(p0, p1);

            // 장력(Tension) 적용
            Vector3 m0 = tangents[i] * (dist * _splineTension);
            Vector3 m1 = tangents[i + 1] * (dist * _splineTension);

            int res = Mathf.Max(1, _curveResolution);
            Vector3 prevPoint = p0;

            for (int j = 1; j <= res; j++)
            {
                float t = j / (float)res;
                Vector3 nextPoint = SplineUtility.GetHermiteCurvePosition(t, p0, p1, m0, m1);

                // 각 구간별로 선을 그어 연결합니다.
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }

        // 제어 포인트 시각화 (노란색 구)
        Gizmos.color = Color.yellow;
        foreach (var kp in keyPoints) Gizmos.DrawSphere(kp, 0.2f);
#endif
    }
}