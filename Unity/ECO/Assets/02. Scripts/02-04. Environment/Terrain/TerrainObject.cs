using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TerrainObject : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Project")]
    [SerializeField]
    private List<TerrainGimmickBaseSO> _gimmickDatas = new List<TerrainGimmickBaseSO>();

    private List<TerrainGimmickBase> _runtimeGimmicks = new List<TerrainGimmickBase>();

    private bool _isEnergyActive;

    private void Awake()
    {
        foreach (var data in _gimmickDatas)
        {
            _runtimeGimmicks.Add(data.CreateGimmick());
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
