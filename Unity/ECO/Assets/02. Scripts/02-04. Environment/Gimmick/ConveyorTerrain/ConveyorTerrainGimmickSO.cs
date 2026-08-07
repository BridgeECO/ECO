using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_ConveyorTerrain", menuName = "Scriptable Objects/Terrain Gimmick/ConveyorTerrainGimmickSO")]
public class ConveyorTerrainGimmickSO : TerrainGimmickBaseSO
{
    [SerializeField]
    private LineRenderer _pathLinePrefab;

    [SerializeField]
    private bool _isPathVisible = true;

    // 기존에는 소유자가 Move/Patrol만 이동 김믹으로 검사해 컨베이어 지형이
    // 이동 김믹으로 분류되지 않는 버그가 있었다. 컨베이어도 지형을 이동시킨다.
    public override bool IsMovementGimmick => true;

    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new ConveyorTerrainGimmick(ActivationType, IsInverted, entry, _pathLinePrefab, _isPathVisible);
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
        if (entry.Waypoints == null || entry.Waypoints.Count < 2)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        for (int i = 0; i < entry.Waypoints.Count - 1; i++)
        {
            Transform from = entry.Waypoints[i];
            Transform to = entry.Waypoints[i + 1];

            if (from == null || to == null)
            {
                continue;
            }

            Gizmos.DrawLine(from.position, to.position);
            Gizmos.DrawSphere(from.position, 0.15f);
        }

        Transform last = entry.Waypoints[entry.Waypoints.Count - 1];

        if (last != null)
        {
            Gizmos.DrawSphere(last.position, 0.15f);
        }
    }
#endif
}
