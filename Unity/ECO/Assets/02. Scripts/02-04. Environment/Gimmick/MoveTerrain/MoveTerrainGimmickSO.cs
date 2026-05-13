using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_MoveTerrain", menuName = "Scriptable Objects/Terrain Gimmick/MoveTerrainGimmickSO")]
public class MoveTerrainGimmickSO : TerrainGimmickBaseSO
{
    [SerializeField]
    private LineRenderer _pathLinePrefab;

    [SerializeField]
    private bool _isPathVisible = true;

    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new MoveTerrainGimmick(ActivationType, IsInverted, entry, _pathLinePrefab, _isPathVisible);
    }

    public override void DrawGizmos(TerrainObject target, TerrainGimmickEntry entry)
    {
#if UNITY_EDITOR
        if (entry.Waypoints != null && 0 < entry.Waypoints.Count)
        {
            Gizmos.color = Color.green;
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
