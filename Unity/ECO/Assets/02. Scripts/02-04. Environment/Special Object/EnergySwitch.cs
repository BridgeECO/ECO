using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergySwitch : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private SwitchSpriteView _spriteView;

    [SerializeField]
    private List<EnergyLine> _connectedLines = new List<EnergyLine>();

    private bool _isOn = false;

    // 원래의 퀘스트 최초 발동 시에만 1회 수행한다.
    private bool _hasTriggeredTracking = false;

    protected override void Awake()
    {
        base.Awake();
        _spriteView?.Refresh(_isOn);
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

        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.PlayWorldSfx(ESfxClip.SE_Energy_Switch, transform);
        }

        for (int i = 0; i < _connectedLines.Count; i++)
        {
            EnergyLine line = _connectedLines[i];
            if (line != null)
            {
                line.SetSwitchState(_isOn);
            }
        }

        if (_isOn)
        {
            RequestTrackingIfNeeded();
        }
    }

    // 최초 On 발동 시, UseTracking이 설정된 첫 번째 라인에 한해 트래킹을 요청한다.
    // EnergyLineTracker의 정적 이벤트로 발행하므로 씬에 Tracker가 없어도 안전하다.
    private void RequestTrackingIfNeeded()
    {
        if (_hasTriggeredTracking)
        {
            return;
        }

        EnergyLine trackingTarget = FindFirstTrackingLine();
        if (trackingTarget == null)
        {
            return;
        }

        _hasTriggeredTracking = true;
        EnergyLineTracker.OnTrackingRequested?.Invoke(trackingTarget);
    }

    private EnergyLine FindFirstTrackingLine()
    {
        for (int i = 0; i < _connectedLines.Count; i++)
        {
            EnergyLine line = _connectedLines[i];
            if (line != null && line.UseTracking)
            {
                return line;
            }
        }
        return null;
    }

    public override void ResetState()
    {
        base.ResetState();
        _hasTriggeredTracking = false;
        SetSwitchState(false);
    }
}
