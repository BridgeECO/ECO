using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergyCore : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<TerrainObject> _registeredTerrains = new List<TerrainObject>();

    public void SetEnergyActive(bool isActive)
    {
        for (int i = 0; i < _registeredTerrains.Count; i++)
        {
            if (_registeredTerrains[i] == null)
            {
                continue;
            }
            _registeredTerrains[i].SetEnergyActive(isActive);
        }
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.yellow;
        for (int i = 0; i < _registeredTerrains.Count; i++)
        {
            if (_registeredTerrains[i] == null)
            {
                continue;
            }
            Vector3 terrainPos = _registeredTerrains[i].transform.position;
            Gizmos.DrawLine(transform.position, terrainPos);
            Gizmos.DrawSphere(terrainPos, 0.2f);
        }
#endif
    }
}
