using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class BossEnergySwitch : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<TerrainObject> _connectedTargets = new List<TerrainObject>();

    // 인라인 직렬화 뷰. 별도 컴포넌트 취득(GetComponent) 없이 인스펙터에서 함께 설정한다.
    [SerializeField]
    private SwitchSpriteView _spriteView = new SwitchSpriteView();

    private bool _isOn = false;

    protected override void Awake()
    {
        base.Awake();
        _spriteView.Refresh(_isOn);
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
        _spriteView.Refresh(_isOn);

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
