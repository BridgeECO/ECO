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

    [Tooltip("공급음과 차단음을 반복 재생하다가 흐름이 끝나면 멈추는 전용 AudioSource입니다.")]
    [SerializeField]
    private AudioSource _sfxAudioSource;

    [Foldout("Settings")]
    [SerializeField]
    private EEnergySupplyPattern _supplyPattern = EEnergySupplyPattern.Always;

    [Tooltip("끄면 처음에는 공급하지 않다가, 연결된 스위치가 처음 작동할 때부터 Supply Pattern을 따릅니다. 리스폰하면 다시 꺼진 채 시작합니다.")]
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
    private EnergySupplyOutput _supplyOutput = new EnergySupplyOutput();
    private EnergySupplyRule _supplyRule = new EnergySupplyRule();

    private bool _isInitPending;

    public IReadOnlyList<EnergyLine> ConnectedLines => _connectedLines;
    public EEnergySupplyPattern SupplyPattern => _supplyPattern;
    public bool IsSupplying => _supplyOutput.IsSupplying;

    private void Awake()
    {
        _supplyOutput.Init(_connectedLines, _spriteView, _sfxAudioSource);
    }

    private void OnEnable()
    {
        _supplyRule.SetDeviceActive(_isActiveOnStart);
        _isInitPending = true;
    }

    private void Update()
    {
        if (_isInitPending)
        {
            ApplyInitialSupply();
            return;
        }

        _supplyOutput.RefreshSfx();
        if (!_supplyRule.IsDeviceActive || _supplyPattern != EEnergySupplyPattern.Periodic)
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
        _supplyRule.SetDeviceActive(false);

        // 리전 언로드처럼 오브젝트가 꺼지면서 공급이 멈출 때는 차단 효과음을 내지 않는다.
        RefreshSupply(false);
    }

    public void SetSwitchSignal(Object sender, bool isOperated)
    {
        if (_supplyRule.SetSwitchSignal(sender, isOperated))
        {
            _supplyCycle.Restart();
        }
        RefreshSupply();
    }

    public void SetDeviceActive(bool isActive)
    {
        if (!_supplyRule.SetDeviceActive(isActive))
        {
            return;
        }

        if (isActive)
        {
            _supplyCycle.Restart();
        }
        RefreshSupply();
    }

    // 같은 프레임에 리셋되는 스위치·지형과 순서가 엇갈려도 결과가 같도록, 선을 비운 뒤 다음 프레임에 초기 공급을 다시 건다.
    public void ResetState()
    {
        _supplyRule.Reset(_isActiveOnStart);
        _supplyOutput.ClearImmediately();
        _isInitPending = true;
    }

    private void ApplyInitialSupply()
    {
        _isInitPending = false;
        _supplyCycle.Init(_supplyDuration, _cutoffDuration);
        _supplyOutput.SetInitialSupply(EvaluateSupply(), _supplyPattern == EEnergySupplyPattern.Periodic);
    }

    private bool EvaluateSupply()
    {
        return _supplyRule.Evaluate(_supplyPattern, _isInverted, _supplyCycle.IsSupplying);
    }

    private void RefreshSupply(bool canPlaySfx = true)
    {
        // 초기 공급을 다시 걸기 전에는 신호만 기록한다. 같은 프레임에 리셋된 스위치가 공급을 먼저 바꾸지 않게 하려는 것이다.
        if (_isInitPending)
        {
            return;
        }

        _supplyOutput.SetSupplying(EvaluateSupply(), canPlaySfx);
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
