using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class BossEnergySwitch : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<TerrainObject> _connectedTargets = new List<TerrainObject>();

    private bool _isOn = false;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Boss)))
        {
            SetSwitchState(true);
            return;
        }
    }
    
    protected override void Interact()
    {
        base.Interact();
        ToggleSwitch();
    }

    protected override void SetState(bool isOn)
    {
        base.SetState(isOn);
        SetSwitchState(isOn);
    }

    private void ToggleSwitch()
    {
        SetSwitchState(!_isOn);
    }

    private void SetSwitchState(bool isOn)
    {
        if (_isOn == isOn)
        {
            return;
        }

        _isOn = isOn;

        foreach (TerrainObject target in _connectedTargets)
        {
            if (target != null)
            {
                Debug.Log($"±â¹Í È°¼ºÈ­");
                target.SetEnergyActive(_isOn);
            }
        }
    }

    public override void ResetState()
    {
        base.ResetState();
        SetSwitchState(false);
    }
}
