using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergySwitch : SpecialObjectBase
{
    // 발판형은 Interact()를 거치지 않고 SetState()로만 오므로 SpecialObjectBase.OnInteract가 발행되지 않는다.
    // 스위치 종류를 가리지 않는 신호는 상태 전이뿐이라, 컷씬 트리거처럼 "눌린 순간"이 필요한 쪽이 이걸 구독한다.
    public Action<bool> OnSwitchStateChanged;

    [Foldout("Hierarchy")]
    [SerializeField]
    private List<EnergyLine> _connectedLines = new List<EnergyLine>();

    // 인라인 직렬화 뷰. 별도 컴포넌트 취득(GetComponent) 없이 인스펙터에서 함께 설정한다.
    [SerializeField]
    private SwitchSpriteView _spriteView = new SwitchSpriteView();

    private bool _isOn = false;

    // 원래의 퀘스트 최초 발동 시에만 1회 수행한다.
    private bool _hasTriggeredTracking = false;

    public bool IsOn => _isOn;

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

        // 상태가 실제로 바뀐 경우에만 온다. 조기 반환 뒤에 두어 발판 재진입과
        // ResetState의 중복 호출이 걸러진다.
        OnSwitchStateChanged?.Invoke(_isOn);
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
