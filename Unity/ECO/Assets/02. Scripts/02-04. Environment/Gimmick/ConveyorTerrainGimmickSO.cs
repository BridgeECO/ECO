using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_ConveyorTerrain", menuName = "Scriptable Objects/Terrain Gimmick/ConveyorTerrainGimmickSO")]
public class ConveyorTerrainGimmickSO : TerrainGimmickBaseSO
{
    [SerializeField]
    private LineRenderer _pathLinePrefab;

    [SerializeField]
    private bool _isPathVisible = true;

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
        if (entry.Waypoints == null || entry.Waypoints.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Vector3 prev = Application.isPlaying ? target.InitialPosition : target.transform.position;

        foreach (var wp in entry.Waypoints)
        {
            if (wp == null)
            {
                continue;
            }

            Gizmos.DrawLine(prev, wp.position);
            Gizmos.DrawSphere(wp.position, 0.15f);
            prev = wp.position;
        }
    }
#endif
}
