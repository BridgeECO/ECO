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

    private bool _isDeviceActive;
    private bool _isSupplying;
    private float _remainingDuration;

    private void Update()
    {
        if (!_isDeviceActive || _supplyPattern != EAutoEnergySupplyPattern.Periodic)
        {
            return;
        }

        UpdateSupplyCycle(Time.deltaTime);
    }

    private void OnDisable()
    {
        StopSupplyCycle();
    }

    public void SetDeviceActive(bool isActive)
    {
        if (_isDeviceActive == isActive)
        {
            return;
        }

        _isDeviceActive = isActive;
        if (_isDeviceActive)
        {
            InitSupplyCycle();
            return;
        }

        StopSupplyCycle();
    }

    public void ResetState()
    {
        StopSupplyCycle();
    }

    private void InitSupplyCycle()
    {
        SetSupplyState(true);
        _remainingDuration = _supplyDuration;
    }

    private void StopSupplyCycle()
    {
        _isDeviceActive = false;
        _remainingDuration = 0f;
        SetSupplyState(false);
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

#if UNITY_EDITOR
    [Button("공급 시작")]
    private void TestStartSupply()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("자동 에너지 공급 장치는 Play Mode에서 테스트할 수 있습니다.", this);
            return;
        }

        SetDeviceActive(true);
    }

    [Button("공급 중단")]
    private void TestStopSupply()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("자동 에너지 공급 장치는 Play Mode에서 테스트할 수 있습니다.", this);
            return;
        }

        SetDeviceActive(false);
    }
#endif
}
