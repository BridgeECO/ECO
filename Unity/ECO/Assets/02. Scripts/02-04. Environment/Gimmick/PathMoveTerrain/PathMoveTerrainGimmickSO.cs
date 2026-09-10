using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_PathMoveTerrain", menuName = "Scriptable Objects/Terrain Gimmick/PathMoveTerrainGimmickSO")]
public class PathMoveTerrainGimmickSO : TerrainGimmickBaseSO
{
    [Tooltip("경로 끝에 도달했을 때의 동작. Stop은 그 자리에 멈추고, PingPong은 방향을 뒤집어 왕복하며, Loop는 첫 웨이포인트로 순간이동해 같은 방향으로 다시 진행합니다.")]
    [SerializeField]
    private EPathEndBehaviour _pathEndBehaviour;

    [Tooltip("에너지가 끊겼을 때 지나온 경로를 역주행해 최초 위치로 되돌아갈지 여부. 끄면 그 자리에 멈춥니다.")]
    [SerializeField]
    private bool _isReturningOnDeactivate;

    [SerializeField]
    private LineRenderer _pathLinePrefab;

    [SerializeField]
    private bool _isPathVisible = true;

    public override bool IsMovementGimmick => true;

    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new PathMoveTerrainGimmick(ActivationType, IsInverted, entry,
            _pathEndBehaviour, _isReturningOnDeactivate, _pathLinePrefab, _isPathVisible);
    }

    public override void DrawGizmos(TerrainObject target, TerrainGimmickEntry entry)
    {
#if UNITY_EDITOR
        DrawPathGizmos(target, entry);
#endif
    }

#if UNITY_EDITOR
    private void DrawPathGizmos(TerrainObject target, TerrainGimmickEntry entry)
    {
        if (entry.Waypoints == null || entry.Waypoints.Count == 0)
        {
            return;
        }

        Gizmos.color = GetGizmoColor();
        Vector3 previous = GetGizmoStartPosition(target, entry);

        for (int i = 0; i < entry.Waypoints.Count; i++)
        {
            Transform waypoint = entry.Waypoints[i];
            if (waypoint == null)
            {
                continue;
            }

            Gizmos.DrawLine(previous, waypoint.position);
            Gizmos.DrawSphere(waypoint.position, 0.15f);
            previous = waypoint.position;
        }
    }

    // Loop은 최초 위치가 아니라 첫 웨이포인트가 경로의 시작점이다.
    private Vector3 GetGizmoStartPosition(TerrainObject target, TerrainGimmickEntry entry)
    {
        if (_pathEndBehaviour == EPathEndBehaviour.Loop && entry.Waypoints[0] != null)
        {
            return entry.Waypoints[0].position;
        }
        return Application.isPlaying ? target.InitialPosition : target.transform.position;
    }

    private Color GetGizmoColor()
    {
        switch (_pathEndBehaviour)
        {
            case EPathEndBehaviour.PingPong:
                return Color.yellow;
            case EPathEndBehaviour.Loop:
                return Color.cyan;
            default:
                return Color.green;
        }
    }
#endif
}
