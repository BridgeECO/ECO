using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 에너지원이 지금 공급해야 하는지 정한다. 에너지원의 켜짐 상태와 작동 중인 스위치를 기록하고 공급 패턴의 규칙을 적용한다.
/// </summary>
public class EnergySupplyRule
{
    // 기획서는 에너지원 하나에 스위치 하나를 전제한다. 여럿이 물리면 하나라도 작동 중일 때 작동한 것으로 본다.
    private List<Object> _operatingSwitches = new List<Object>();

    public bool IsDeviceActive { get; private set; }

    public void Reset(bool isActiveOnStart)
    {
        _operatingSwitches.Clear();
        IsDeviceActive = isActiveOnStart;
    }

    public void SetSwitchSignal(Object sender, bool isOperated)
    {
        bool isRegistered = _operatingSwitches.Contains(sender);
        if (isOperated && !isRegistered)
        {
            _operatingSwitches.Add(sender);
        }
        else if (!isOperated && isRegistered)
        {
            _operatingSwitches.Remove(sender);
        }
    }

    // 켜짐 상태가 실제로 바뀌었을 때만 true를 돌려준다.
    public bool SetDeviceActive(bool isActive)
    {
        if (IsDeviceActive == isActive)
        {
            return false;
        }

        IsDeviceActive = isActive;
        return true;
    }

    public bool Evaluate(EEnergySupplyPattern pattern, bool isInverted, bool isCycleSupplying)
    {
        if (!IsDeviceActive)
        {
            return false;
        }

        switch (pattern)
        {
            case EEnergySupplyPattern.Periodic:
                return isCycleSupplying;
            case EEnergySupplyPattern.Switch:
                bool isOperated = 0 < _operatingSwitches.Count;
                return isInverted ? !isOperated : isOperated;
            default:
                return true;
        }
    }
}
