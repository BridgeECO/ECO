using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TerrainObject : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Project")]
    [SerializeField]
    private List<TerrainGimmickEntry> _gimmickEntries = new List<TerrainGimmickEntry>();

    private List<TerrainGimmickBase> _runtimeGimmicks = new List<TerrainGimmickBase>();

    [Foldout("Energy")]
    [SerializeField]
    private Transform _activationPosition;

    [SerializeField]
    private Transform _deactivationPosition;

    public Transform ActivationPosition => _activationPosition;
    public Transform DeactivationPosition => _deactivationPosition;

    private bool _isEnergyActive;

    private void Awake()
    {
        foreach (var entry in _gimmickEntries)
        {
            if (entry.GimmickData != null)
            {
                _runtimeGimmicks.Add(entry.GimmickData.CreateGimmick(entry));
            }
        }
        ApplyGimmicks();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            gimmick.OnTerrainTriggerEnter2D(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            gimmick.OnTerrainTriggerExit2D(other);
        }
    }

    private void OnDestroy()
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            gimmick?.OnDestroy(this);
        }
    }

    private void ApplyGimmicks()
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            if (gimmick != null)
            {
                gimmick.Evaluate(this, _isEnergyActive);
            }
        }
    }

    public void SetEnergyActive(bool isActive)
    {
        _isEnergyActive = isActive;
        ApplyGimmicks();
    }
}
