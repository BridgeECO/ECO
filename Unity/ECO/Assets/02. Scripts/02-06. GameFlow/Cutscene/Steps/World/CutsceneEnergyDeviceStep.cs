using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>자동 에너지 공급 장치를 가동하거나 멈춘다.</summary>
[Serializable]
[Preserve]
public class CutsceneEnergyDeviceStep : CutsceneStepBase
{
    [SerializeField]
    private List<AutoEnergySupplyDevice> _devices = new List<AutoEnergySupplyDevice>();

    [SerializeField]
    private bool _isDeviceActive = true;

    public override UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        Apply();
        return UniTask.CompletedTask;
    }

    public override void ApplyFinalState(CutsceneContext context)
    {
        Apply();
    }

    private void Apply()
    {
        for (int i = 0; i < _devices.Count; i++)
        {
            AutoEnergySupplyDevice device = _devices[i];
            if (device != null)
            {
                device.SetDeviceActive(_isDeviceActive);
            }
        }
    }
}
