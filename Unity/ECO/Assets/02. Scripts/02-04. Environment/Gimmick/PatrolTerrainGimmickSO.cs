using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_PatrolTerrain", menuName = "Scriptable Objects/Terrain Gimmick/PatrolTerrainGimmickSO")]
public class PatrolTerrainGimmickSO : TerrainGimmickBaseSO
{
    [SerializeField]
    private LineRenderer _pathLinePrefab;

    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new PatrolTerrainGimmick(ActivationType, IsInverted, entry, _pathLinePrefab);
    }

    public override void DrawGizmos(TerrainObject target, TerrainGimmickEntry entry)
    {
#if UNITY_EDITOR
        if (entry.Waypoints != null && 0 < entry.Waypoints.Count)
        {
            Gizmos.color = Color.yellow;
            Vector3 prev = Application.isPlaying ? target.InitialPosition : target.transform.position;
            foreach (var wp in entry.Waypoints)
            {
                if (wp != null)
                {
                    Gizmos.DrawLine(prev, wp.position);
                    Gizmos.DrawSphere(wp.position, 0.15f);
                    prev = wp.position;
                }
            }
        }
#endif
    }
}
