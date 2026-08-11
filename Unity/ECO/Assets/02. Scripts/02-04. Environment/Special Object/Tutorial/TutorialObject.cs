using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 인식 범위와 게임 조건을 함께 만족했을 때 맵에 조작 안내를 띄우는 오브젝트.
/// 범위 감지와 안내 출력을 맡고, 발동 판정은 TutorialTrigger에 위임한다.
///
/// SpecialObjectBase를 상속하지 않는다. 그쪽이 강제하는 InteractionBase 4종은 모두
/// 범위 이탈 시 표시를 끄는데, 튜토리얼은 "조건이 깨질 때만" 사라져야 해서 규칙이 충돌하고
/// Interact/SetState 계약도 쓸 일이 없다. 범위 감지만 필요한 GimmickTriggerZone과 같은 선택이다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TutorialObject : MonoBehaviour, IResettable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Tutorial _uiTutorial;

    [Foldout("Project")]
    [Header("Content")]
    [SerializeField]
    [TextArea(2, 5)]
    private string _tutorialText;

    [SerializeField]
    private Sprite _tutorialImage;

    // 발동 조건. 판정 로직은 TutorialTrigger가 맡지만 리스트는 여기서 소유한다.
    // 트리거 쪽에 두면 인스펙터가 Trigger > Condition Set > Conditions로 불필요하게 깊어진다.
    [Header("Condition")]
    [SerializeReference]
    private List<TutorialConditionBase> _conditions = new List<TutorialConditionBase>();

    private TutorialTrigger _trigger;
    private bool _isPlayerInRange;
    private bool _isListenerAdded;

    // 조건 리스트는 역직렬화가 끝나야 확정되므로 필드 초기화 시점에는 넘길 수 없다.
    // 비활성 상태로 배치된 오브젝트도 Room.ResetRoom의 대상이 되어 Awake 없이 호출될 수 있어,
    // 첫 접근 시점에 만든다.
    private TutorialTrigger Trigger
    {
        get
        {
            if (ReferenceEquals(_trigger, null))
            {
                _trigger = new TutorialTrigger(_conditions);
            }

            return _trigger;
        }
    }

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        AddEventListeners();
    }

    // EventManager가 아직 없는 로드 순서에서도 구독이 성사되도록 한 번 더 시도한다.
    private void Start()
    {
        AddEventListeners();
    }

    private void Update()
    {
        if (Trigger.Tick(_isPlayerInRange, Time.deltaTime))
        {
            RefreshTutorial();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            return;
        }

        _isPlayerInRange = true;

        // 표시 중에는 범위를 벗어나도 참조와 조건 상태를 그대로 들고 있다(OnTriggerExit2D 참고).
        // 여기서 다시 바인딩하면 조건이 초기화되어 다음 프레임에 곧바로 "깨진 것"으로 읽히고,
        // 플레이어가 조작을 익히지 않았는데도 발동이 끝나버린다.
        if (Trigger.IsVisible)
        {
            return;
        }

        // 발동이 끝난 상태에서도 바인딩해 둔다. 리스폰으로 감시가 되살아났을 때
        // 플레이어가 이미 범위 안이면 진입 이벤트가 다시 오지 않기 때문이다.
        BindPlayer(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            return;
        }

        _isPlayerInRange = false;

        // 표시 중이라면 범위를 벗어나도 조건 평가를 계속해야 하므로 참조를 놓지 않는다.
        if (Trigger.IsVisible)
        {
            return;
        }

        Trigger.Release();
    }

    private void OnDisable()
    {
        RemoveEventListeners();
        _isPlayerInRange = false;
        Trigger.Suspend();
        HideTutorialImmediate();
    }
    #endregion

    #region Event Handlers
    private void OnSavePointUpdated()
    {
        Trigger.SaveProgress();
    }
    #endregion

    public void ResetState()
    {
        // 리스폰 중이라 플레이어가 보고 있지 않다. 사라지는 연출을 재생할 이유가 없다.
        HideTutorialImmediate();
        Trigger.ResetToSavedState(_isPlayerInRange);
    }

    private void RefreshTutorial()
    {
        if (_uiTutorial == null)
        {
            return;
        }

        if (Trigger.IsVisible)
        {
            _uiTutorial.ShowTutorial(_tutorialText, _tutorialImage);
            return;
        }

        _uiTutorial.HideTutorial();
    }

    private void HideTutorialImmediate()
    {
        if (_uiTutorial != null)
        {
            _uiTutorial.HideTutorialImmediate();
        }
    }

    // 플레이어 전역 접근점이 없고 Find 계열 탐색이 금지되어 있어,
    // 범위에 들어온 콜라이더에서 역으로 참조를 수집한다. (NPC와 동일한 방식)
    private void BindPlayer(Collider2D playerCollider)
    {
        PlayerInput input = playerCollider.GetComponentInParent<PlayerInput>();
        PlayerStateMachine stateMachine = playerCollider.GetComponentInParent<PlayerStateMachine>();
        PlayerSensor sensor = playerCollider.GetComponentInParent<PlayerSensor>();
        Transform playerTransform = input != null ? input.transform : playerCollider.transform;

        Trigger.Bind(new TutorialConditionContext(input, stateMachine, sensor, playerTransform));
    }

    private void AddEventListeners()
    {
        if (_isListenerAdded || EventManager.Instance == null)
        {
            return;
        }

        EventManager.Instance.AddEventListener(EEventType.SavePointUpdated, OnSavePointUpdated);
        _isListenerAdded = true;
    }

    private void RemoveEventListeners()
    {
        if (!_isListenerAdded)
        {
            return;
        }

        _isListenerAdded = false;
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.SavePointUpdated, OnSavePointUpdated);
        }
    }
}
