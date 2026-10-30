using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 연결된 에너지선에 에너지를 공급하는 오브젝트. 공급 여부는 선의 SetSwitchState로 전달한다.
/// 스위치 작동형은 지형 기믹과 같은 규칙을 따른다. 신호가 켜지면 공급하고, 반전이면 반대로 동작한다.
/// </summary>
public class EnergySource : MonoBehaviour, IResettable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<EnergyLine> _connectedLines = new List<EnergyLine>();

    // 인라인 직렬화 뷰. 공급 중일 때와 아닐 때 스프라이트를 바꾼다.
    [SerializeField]
    private SwitchSpriteView _spriteView = new SwitchSpriteView();

    [Foldout("Settings")]
    [SerializeField]
    private EEnergySupplyPattern _supplyPattern = EEnergySupplyPattern.Always;

    [Tooltip("켜 두면 씬이 시작될 때부터 공급 규칙을 따릅니다. 끄면 SetDeviceActive(true)로 켤 때까지 공급하지 않습니다.")]
    [SerializeField]
    private bool _isActiveOnStart = true;

    [ShowIf(nameof(_supplyPattern), EEnergySupplyPattern.Switch)]
    [Tooltip("켜면 연결된 스위치가 작동하는 동안 공급을 멈춥니다.")]
    [SerializeField]
    private bool _isInverted;

    [ShowIf(nameof(_supplyPattern), EEnergySupplyPattern.Periodic)]
    [Min(0.1f)]
    [SerializeField]
    private float _supplyDuration = 3f;

    [ShowIf(nameof(_supplyPattern), EEnergySupplyPattern.Periodic)]
    [Min(0.1f)]
    [SerializeField]
    private float _cutoffDuration = 3f;

    private EnergySupplyCycle _supplyCycle = new EnergySupplyCycle();

    // 기획서는 에너지원 하나에 스위치 하나를 전제한다. 여럿이 물리면 하나라도 작동 중일 때 작동한 것으로 본다.
    private List<Object> _operatingSwitches = new List<Object>();

    private bool _isDeviceActive;
    private bool _isSupplying;
    private bool _isInitPending;

    public IReadOnlyList<EnergyLine> ConnectedLines => _connectedLines;
    public EEnergySupplyPattern SupplyPattern => _supplyPattern;
    public bool IsSupplying => _isSupplying;

    private void OnEnable()
    {
        _isDeviceActive = _isActiveOnStart;
        _isInitPending = true;
    }

    private void Update()
    {
        if (_isInitPending)
        {
            ApplyInitialSupply();
            return;
        }

        if (!_isDeviceActive || _supplyPattern != EEnergySupplyPattern.Periodic)
        {
            return;
        }

        if (_supplyCycle.Tick(Time.deltaTime))
        {
            RefreshSupply();
        }
    }

    private void OnDisable()
    {
        _isDeviceActive = false;
        RefreshSupply();
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

        RefreshSupply();
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
            _supplyCycle.Restart();
        }
        RefreshSupply();
    }

    // 같은 프레임에 리셋되는 스위치·지형과 순서가 엇갈려도 결과가 같도록, 선을 비운 뒤 다음 프레임에 초기 공급을 다시 건다.
    public void ResetState()
    {
        _operatingSwitches.Clear();
        _isDeviceActive = _isActiveOnStart;
        _isSupplying = false;
        for (int i = 0; i < _connectedLines.Count; i++)
        {
            if (_connectedLines[i] != null)
            {
                _connectedLines[i].ResetEnergy();
            }
        }
        _isInitPending = true;
    }

    // 상시형과 차단형은 흘려보내는 연출 없이 가득 찬 선으로 시작하고, 주기형은 공급 구간부터 새로 흘려보낸다.
    private void ApplyInitialSupply()
    {
        _isInitPending = false;
        _supplyCycle.Init(_supplyDuration, _cutoffDuration);
        _isSupplying = EvaluateSupply();
        _spriteView.Refresh(_isSupplying);
        if (!_isSupplying)
        {
            return;
        }

        for (int i = 0; i < _connectedLines.Count; i++)
        {
            EnergyLine line = _connectedLines[i];
            if (line == null)
            {
                continue;
            }

            if (_supplyPattern == EEnergySupplyPattern.Periodic)
            {
                line.SetSwitchState(true);
                continue;
            }
            line.FillImmediately();
        }
    }

    private bool EvaluateSupply()
    {
        if (!_isDeviceActive)
        {
            return false;
        }

        switch (_supplyPattern)
        {
            case EEnergySupplyPattern.Periodic:
                return _supplyCycle.IsSupplying;
            case EEnergySupplyPattern.Switch:
                bool isOperated = 0 < _operatingSwitches.Count;
                return _isInverted ? !isOperated : isOperated;
            default:
                return true;
        }
    }

    private void RefreshSupply()
    {
        // 초기 공급을 다시 걸기 전에는 신호만 기록한다. 같은 프레임에 리셋된 스위치가 공급을 먼저 바꾸지 않게 하려는 것이다.
        if (_isInitPending)
        {
            return;
        }

        bool isSupplying = EvaluateSupply();
        if (_isSupplying == isSupplying)
        {
            return;
        }

        _isSupplying = isSupplying;
        _spriteView.Refresh(_isSupplying);
        for (int i = 0; i < _connectedLines.Count; i++)
        {
            if (_connectedLines[i] != null)
            {
                _connectedLines[i].SetSwitchState(_isSupplying);
            }
        }
    }

#if UNITY_EDITOR
    [Button("공급 시작")]
    private void TestStartSupply()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("에너지원은 Play Mode에서 테스트할 수 있습니다.", this);
            return;
        }

        SetDeviceActive(true);
    }

    [Button("공급 중단")]
    private void TestStopSupply()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("에너지원은 Play Mode에서 테스트할 수 있습니다.", this);
            return;
        }

        SetDeviceActive(false);
    }
#endif
}
