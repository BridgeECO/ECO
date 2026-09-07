using UnityEngine;
using VInspector;

/// <summary>
/// 컷씬을 발동시킨다.
///
/// 발판형 스위치는 Interact()를 거치지 않고 SetState()로만 오므로 SpecialObjectBase.OnInteract가
/// 발행되지 않는다. 스위치 종류를 가리지 않는 신호는 상태 전이뿐이라 그쪽을 구독한다.
///
/// 자체 콜라이더는 발동용이 아니라 플레이어 참조를 캐싱하는 용도다. 발판은 발바닥
/// 콜라이더(PlayerFeet)에만 반응하는데 트리거 존이 쓸 수 있는 태그의 콜라이더는 어느 쪽도
/// 발판 높이에 닿지 않는다. 발판에 맞춘 존은 영영 발화하지 않고, 발화하도록 키운 존은
/// "밟았을 때"가 아니라 "지나갈 때" 터진다. 두 역할을 나눠야 그 문제가 사라진다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CutsceneTrigger : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private CutsceneDirector _director;

    [SerializeField]
    [Tooltip("이 스위치가 켜지면 컷씬을 시작합니다. 비워 두면 범위에 들어오는 것만으로 시작합니다.")]
    private EnergySwitch _triggerSwitch;

    private bool _isSubscribed;

    private void Awake()
    {
        if (_director == null)
        {
            _director = GetComponent<CutsceneDirector>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)) || _director == null)
        {
            return;
        }

        // 참조만 캐싱한다. 스위치를 지정했으면 발동은 그쪽이 알린다.
        _director.BindPlayerFrom(other);

        if (_triggerSwitch == null)
        {
            _director.Play();
        }
    }

    private void Subscribe()
    {
        if (_isSubscribed || _triggerSwitch == null)
        {
            return;
        }

        _triggerSwitch.OnSwitchStateChanged += HandleSwitchStateChanged;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
        {
            return;
        }

        _isSubscribed = false;
        if (_triggerSwitch != null)
        {
            _triggerSwitch.OnSwitchStateChanged -= HandleSwitchStateChanged;
        }
    }

    private void HandleSwitchStateChanged(bool isOn)
    {
        // ResetState가 유발하는 true 에서 false 로의 발화는 무시한다.
        if (!isOn || _director == null)
        {
            return;
        }

        _director.Play();
    }
}
