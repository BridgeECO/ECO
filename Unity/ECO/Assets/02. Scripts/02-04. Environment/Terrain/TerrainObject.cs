using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TerrainObject : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Project")]
    [SerializeField]
    private ETerrainState _terrainState = ETerrainState.Active;

    [SerializeField]
    private List<TerrainGimmickBaseSO> _gimmickDatas = new List<TerrainGimmickBaseSO>();

    private List<TerrainGimmickBase> _runtimeGimmicks = new List<TerrainGimmickBase>();

    private bool _isEnergyActive = false;

    private void Awake()
    {
        foreach (var data in _gimmickDatas)
        {
            _runtimeGimmicks.Add(data.CreateGimmick());
        }

        RefreshTerrainState();
    }

    public void SetEnergyActive(bool isActive)
    {
        _isEnergyActive = isActive;
        RefreshTerrainState();
    }

    private void RefreshTerrainState()
    {
        bool isActiveState = false;
        switch (_terrainState)
        {
            case ETerrainState.Always:
                isActiveState = true;
                break;
            case ETerrainState.Active:
                isActiveState = _isEnergyActive;
                break;
            case ETerrainState.Inactive:
                isActiveState = !_isEnergyActive;
                break;
        }
        ApplyGimmicks(isActiveState);
    }

    private void ApplyGimmicks(bool isActive)
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            if (gimmick != null)
            {
                gimmick.ApplyGimmick(this, isActive);
            }
        }
    }
}
