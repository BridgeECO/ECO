using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class BossEnergySwitch : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private SwitchSpriteView _spriteView;

    [SerializeField]
    private List<TerrainObject> _connectedTargets = new List<TerrainObject>();

    private bool _isOn = false;

    protected override void Awake()
    {
        base.Awake();
        _spriteView?.Refresh(_isOn);
    }

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
        _spriteView?.Refresh(_isOn);

        foreach (TerrainObject target in _connectedTargets)
        {
            if (target != null)
            {
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
