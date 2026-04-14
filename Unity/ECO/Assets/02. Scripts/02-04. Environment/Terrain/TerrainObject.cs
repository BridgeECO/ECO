using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TerrainObject : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Project")]
    [SerializeField]
    private List<TerrainGimmickEntry> _gimmickEntries = new List<TerrainGimmickEntry>();

    private List<TerrainGimmickBase> _runtimeGimmicks = new List<TerrainGimmickBase>();

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

    public void SetEnergyActive(bool isActive)
    {
        _isEnergyActive = isActive;
        ApplyGimmicks();
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
}
