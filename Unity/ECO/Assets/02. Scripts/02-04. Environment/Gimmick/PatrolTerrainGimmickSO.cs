using UnityEngine;

[CreateAssetMenu(fileName = "PatrolTerrainGimmick", menuName = "Environment/Gimmick/PatrolTerrainGimmick", order = 2)]
public class PatrolTerrainGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new PatrolTerrainGimmick(ActivationType, IsInverted, entry);
    }

    public override void DrawGizmos(TerrainObject target, TerrainGimmickEntry entry)
    {
#if UNITY_EDITOR
        if (entry.Waypoints != null && entry.Waypoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 prev = target.transform.position;
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
