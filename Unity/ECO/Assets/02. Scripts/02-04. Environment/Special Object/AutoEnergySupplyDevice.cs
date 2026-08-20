using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class AutoEnergySupplyDevice : MonoBehaviour, IResettable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<EnergyLine> _connectedLines = new List<EnergyLine>();

    [Foldout("Supply")]
    [SerializeField]
    private EAutoEnergySupplyPattern _supplyPattern = EAutoEnergySupplyPattern.Always;

    [ShowIf(nameof(_supplyPattern), EAutoEnergySupplyPattern.Periodic)]
    [Min(0.1f)]
    [SerializeField]
    private float _supplyDuration = 3f;

    [ShowIf(nameof(_supplyPattern), EAutoEnergySupplyPattern.Periodic)]
    [Min(0.1f)]
    [SerializeField]
    private float _cutoffDuration = 3f;

    private bool _isSupplying;
    private float _remainingDuration;

    private void OnEnable()
    {
        InitSupplyCycle();
    }

    private void Update()
    {
        if (_supplyPattern != EAutoEnergySupplyPattern.Periodic)
        {
            return;
        }

        UpdateSupplyCycle(Time.deltaTime);
    }

    private void OnDisable()
    {
        SetSupplyState(false);
    }

    public void SetDeviceActive(bool isActive)
    {
        enabled = isActive;
    }

    public void ResetState()
    {
        if (!isActiveAndEnabled)
        {
            SetSupplyState(false);
            return;
        }

        InitSupplyCycle();
    }

    private void InitSupplyCycle()
    {
        SetSupplyState(true);
        _remainingDuration = _supplyDuration;
    }

    private void UpdateSupplyCycle(float deltaTime)
    {
        _remainingDuration -= deltaTime;
        if (0f < _remainingDuration)
        {
            return;
        }

        SetSupplyState(!_isSupplying);
        _remainingDuration = _isSupplying ? _supplyDuration : _cutoffDuration;
    }

    private void SetSupplyState(bool isSupplying)
    {
        if (_isSupplying == isSupplying)
        {
            return;
        }

        _isSupplying = isSupplying;
        for (int i = 0; i < _connectedLines.Count; i++)
        {
            EnergyLine line = _connectedLines[i];
            if (line != null)
            {
                line.SetSwitchState(_isSupplying);
            }
        }
    }
}
