using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class BossChasingLine : MonoBehaviour
{
    private const float DEFAULT_SPEED_MULTIPLIER = 1f;

    [Foldout("Hierarchy")]
    [Header("Path")]
    [SerializeField]
    [Tooltip("경로의 시작점. 비워두면 이 오브젝트의 위치를 사용합니다.")]
    private Transform _startPoint;

    [SerializeField]
    [Tooltip("이전 지점에서 도착점까지의 경로와 배속을 순서대로 설정합니다.")]
    private List<BossPathSection> _sections = new List<BossPathSection>();

    [Foldout("Path Settings")]
    [SerializeField]
    [Tooltip("각 구간의 곡선 분할 해상도입니다.")]
    private int _curveResolution = 10;

    [SerializeField]
    [Tooltip("스플라인 곡선의 장력입니다.")]
    private float _splineTension = 1f;

    [HideInInspector]
    [SerializeField]
    private List<Transform> _waypoints = new List<Transform>();

    [HideInInspector]
    [SerializeField]
    private Transform _endPoint;

    [HideInInspector]
    [SerializeField]
    private List<float> _speedMultipliers = new List<float>();

    private List<BossPathPoint> _computedPaths = new List<BossPathPoint>();

    private void Awake()
    {
        MigrateLegacySections();
        CalculatePath();
    }

    private void OnValidate()
    {
        MigrateLegacySections();
        CalculatePath();
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        MigrateLegacySections();
        List<Vector3> keyPoints = new List<Vector3>();
        CollectKeyPoints(keyPoints);

        if (keyPoints.Count < 2)
        {
            return;
        }

        Vector3[] tangents = SplineUtility.CalculateTangents(keyPoints);
        int resolution = Mathf.Max(1, _curveResolution);

        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            BossPathGizmoDrawer.DrawSection(
                keyPoints,
                tangents,
                i,
                resolution,
                _splineTension);
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < keyPoints.Count; i++)
        {
            Gizmos.DrawSphere(keyPoints[i], 0.2f);
        }
#endif
    }

    public IReadOnlyList<BossPathPoint> GetComputedPath()
    {
        if (_computedPaths.Count == 0)
        {
            CalculatePath();
        }

        return _computedPaths;
    }

    public void CalculatePath()
    {
        _computedPaths.Clear();

        List<Vector3> keyPoints = new List<Vector3>();
        CollectKeyPoints(keyPoints);

        if (keyPoints.Count < 2)
        {
            return;
        }

        Vector3[] tangents = SplineUtility.CalculateTangents(keyPoints);
        _computedPaths.Add(new BossPathPoint(keyPoints[0], GetSpeedMultiplier(0)));

        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            AddSectionPoints(keyPoints, tangents, i);
        }
    }

    private void MigrateLegacySections()
    {
        BossPathSectionMigration.Populate(
            _sections,
            _waypoints,
            _endPoint,
            _speedMultipliers);
    }

    private void CollectKeyPoints(List<Vector3> keyPoints)
    {
        keyPoints.Add(_startPoint != null ? _startPoint.position : transform.position);

        for (int i = 0; i < _sections.Count; i++)
        {
            BossPathSection section = _sections[i];
            if (section is null)
            {
                continue;
            }

            Transform endPoint = section.EndPoint;
            if (endPoint != null)
            {
                keyPoints.Add(endPoint.position);
            }
        }
    }

    private void AddSectionPoints(IReadOnlyList<Vector3> keyPoints, IReadOnlyList<Vector3> tangents, int sectionIndex)
    {
        Vector3 startPosition = keyPoints[sectionIndex];
        Vector3 endPosition = keyPoints[sectionIndex + 1];
        float distance = Vector3.Distance(startPosition, endPosition);
        Vector3 startTangent = tangents[sectionIndex] * (distance * _splineTension);
        Vector3 endTangent = tangents[sectionIndex + 1] * (distance * _splineTension);
        float speedMultiplier = GetSpeedMultiplier(sectionIndex);
        int resolution = Mathf.Max(1, _curveResolution);

        for (int i = 1; i <= resolution; i++)
        {
            float interpolation = i / (float)resolution;
            Vector3 position = SplineUtility.GetHermiteCurvePosition(
                interpolation,
                startPosition,
                endPosition,
                startTangent,
                endTangent);

            _computedPaths.Add(new BossPathPoint(position, speedMultiplier));
        }
    }

    private float GetSpeedMultiplier(int sectionIndex)
    {
        int validSectionIndex = 0;
        for (int i = 0; i < _sections.Count; i++)
        {
            BossPathSection section = _sections[i];
            if (section is null || section.EndPoint == null)
            {
                continue;
            }

            if (validSectionIndex == sectionIndex)
            {
                return section.SpeedMultiplier;
            }

            validSectionIndex++;
        }

        return DEFAULT_SPEED_MULTIPLIER;
    }

}
