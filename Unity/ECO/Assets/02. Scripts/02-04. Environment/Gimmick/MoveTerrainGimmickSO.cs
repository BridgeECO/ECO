using UnityEngine;

[CreateAssetMenu(fileName = "MoveTerrainGimmick", menuName = "Environment/Gimmick/MoveTerrainGimmick", order = 1)]
public class MoveTerrainGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new MoveTerrainGimmick(ActivationType, IsInverted, entry);
    }

    public override void DrawGizmos(TerrainObject target, TerrainGimmickEntry entry)
    {
#if UNITY_EDITOR
        if (entry.Waypoints != null && entry.Waypoints.Count > 0)
        {
            Gizmos.color = Color.cyan;
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
