using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergyCore : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<TerrainObject> _registeredTerrains = new List<TerrainObject>();

    [Foldout("Energy")]
    [SerializeField]
    private Transform _activationPosition;

    [SerializeField]
    private Transform _deactivationPosition;

    public Transform ActivationPosition => _activationPosition;
    public Transform DeactivationPosition => _deactivationPosition;

    private SpriteRenderer _spriteRenderer;
    private Color _activeColor = Color.yellow;
    private Color _deactiveColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _deactiveColor = _spriteRenderer.color;
    }

    public void SetEnergyActive(bool isActive)
    {
        _spriteRenderer.color = isActive ? _activeColor : _deactiveColor;
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
