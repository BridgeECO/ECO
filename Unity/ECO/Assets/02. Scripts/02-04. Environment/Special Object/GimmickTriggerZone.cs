using System.Collections.Generic;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Collider2D))]
public class GimmickTriggerZone : MonoBehaviour, IResettable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<TerrainObject> _targetObjects;

    [Foldout("Project")]
    [SerializeField]
    private bool _isOneShot;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            return;
        }

        for (int i = 0; i < _targetObjects.Count; i++)
        {
            if (_targetObjects[i] != null)
            {
                _targetObjects[i].SetEnergyActive(true);
            }
        }

        if (_isOneShot)
        {
            enabled = false;
        }
    }

    public void ResetState()
    {
        if (_isOneShot)
        {
            enabled = true;
        }
    }
}
