using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergySwitch : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [Tooltip("이 스위치가 공급 여부를 정하는 에너지원입니다. 에너지선과 물리적으로 연결할 필요가 없습니다.")]
    [SerializeField]
    private List<EnergySource> _targetSources = new List<EnergySource>();

    // 에너지원이 생기기 전 방식으로 선에 직접 물린 연결. 기존 룸을 변환 툴로 옮길 때까지만 남겨 둔다.
    [HideInInspector]
    [SerializeField]
    private List<EnergyLine> _connectedLines = new List<EnergyLine>();

    // 인라인 직렬화 뷰. 별도 컴포넌트 취득(GetComponent) 없이 인스펙터에서 함께 설정한다.
    [SerializeField]
    private SwitchSpriteView _spriteView = new SwitchSpriteView();

    private bool _isOn = false;

    // 원래의 퀘스트 최초 발동 시에만 1회 수행한다.
    private bool _hasTriggeredTracking = false;

    public bool IsOn => _isOn;
    public IReadOnlyList<EnergySource> TargetSources => _targetSources;
    public IReadOnlyList<EnergyLine> LegacyConnectedLines => _connectedLines;

    protected override void Awake()
    {
        base.Awake();
        _spriteView.Refresh(_isOn);
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

        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.PlayWorldSfx(ESfxClip.SE_Energy_Switch, transform);
        }

        for (int i = 0; i < _targetSources.Count; i++)
        {
            if (_targetSources[i] != null)
            {
                _targetSources[i].SetSwitchSignal(this, _isOn);
            }
        }

        for (int i = 0; i < _connectedLines.Count; i++)
        {
            if (_connectedLines[i] != null)
            {
                _connectedLines[i].SetSwitchState(_isOn);
            }
        }

        if (_isOn)
        {
            RequestTrackingIfNeeded();
        }
    }

    // 최초 On 발동 시, UseTracking이 설정된 첫 번째 라인에 한해 트래킹을 요청한다.
    // Tracker는 PersistentScene에 있어 씬 간 직접 참조가 불가능하므로 이벤트로 발행한다.
    // (스위치가 EnergyLineTracker 타입을 직접 알지 않게 하기 위함)
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
        EventManager.Instance.BroadcastEvent(EEventType.EnergyLineTrackingRequested, trackingTarget);
    }

    // 스위치는 선을 직접 모르므로, 대상 에너지원이 공급하는 선에서 찾는다.
    private EnergyLine FindFirstTrackingLine()
    {
        for (int i = 0; i < _targetSources.Count; i++)
        {
            if (_targetSources[i] == null)
            {
                continue;
            }

            EnergyLine line = FindTrackingLine(_targetSources[i].ConnectedLines);
            if (line != null)
            {
                return line;
            }
        }

        return FindTrackingLine(_connectedLines);
    }

    private EnergyLine FindTrackingLine(IReadOnlyList<EnergyLine> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] != null && lines[i].UseTracking)
            {
                return lines[i];
            }
        }
        return null;
    }

    public override void ResetState()
    {
        base.ResetState();
        _hasTriggeredTracking = false;
        SetSwitchState(false);

        // 선에 직접 물린 스위치는 리스폰 때 흘려보내는 연출 없이 선을 바로 비운다. 에너지원은 스스로 리셋된다.
        for (int i = 0; i < _connectedLines.Count; i++)
        {
            if (_connectedLines[i] != null)
            {
                _connectedLines[i].ResetEnergy();
            }
        }
    }
}
