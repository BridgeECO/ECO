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
        foreach (var terrain in _registeredTerrains)
        {
            if (terrain == null)
            {
                continue;
            }
            terrain.SetEnergyActive(isActive);
        }
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.yellow;
        foreach (var terrain in _registeredTerrains)
        {
            if (terrain == null)
            {
                continue;
            }
            Vector3 terrainPos = terrain.transform.position;
            Gizmos.DrawLine(transform.position, terrainPos);
            Gizmos.DrawSphere(terrainPos, 0.2f);
        }
#endif
    }
}
