using System;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Collider2D))]
public abstract class SpecialObjectBase : MonoBehaviour, IResettable, IInteractionTarget, IInteractionCandidate
{
    public Action OnInteract;
    public Action<bool> OnSelectedChanged;

    [Foldout("Project")]
    [SerializeField]
    protected EInteractionType _interactionType;

    [Foldout("Project")]
    [ShowIf(nameof(_interactionType), EInteractionType.ButtonOneShotTimer)]
    [SerializeField]
    private float _buttonOneShotDuration = 3f;

    private InteractionBase _interaction;
    private PlayerInteractTargetSelector _selector;
    private Collider2D _interactor;

    public EInteractionType InteractionType { get => _interactionType; private set => _interactionType = value; }

    public bool IsPlayerInRange { get; private set; }

    public bool IsSelected { get; private set; }

    protected virtual void Awake()
    {
        // 파생 Interaction 구체 타입 선택은 팩토리가 담당한다. (베이스→파생 의존 제거)
        _interaction = InteractionFactory.Create(_interactionType, this, _buttonOneShotDuration);
    }

    protected virtual void OnDisable()
    {
        UnregisterFromSelector();
        _interactor = null;
        _interaction?.Dispose();
        IsPlayerInRange = false;
    }

    public virtual void ResetState()
    {
        IsPlayerInRange = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidInteractor(other))
        {
            return;
        }

        IsPlayerInRange = true;

        // 버튼식은 범위에 들어와도 바로 상호작용하지 않고 후보로만 등록한다.
        // 표시와 F 입력은 선택기가 이 오브젝트를 고를 때(SetSelected) 함께 켜진다.
        if (_interaction is not null && _interaction.IsSelectionRequired && other.TryGetComponent(out PlayerInteractTargetSelector selector))
        {
            _selector = selector;
            _interactor = other;
            _selector.Register(this);
            return;
        }

        EnterInteraction(other);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidInteractor(other))
        {
            return;
        }

        IsPlayerInRange = false;

        if (_selector != null)
        {
            UnregisterFromSelector();
            _interactor = null;
            return;
        }

        ExitInteraction(other);
    }

    protected bool IsValidInteractor(Collider2D other)
    {
        string validTag = _interaction != null ? _interaction.ValidTag : nameof(ETags.Player);
        return other.CompareTag(validTag);
    }

    protected virtual void HandlePlayerEnter()
    {
    }

    protected virtual void HandlePlayerExit()
    {
    }

    // Interaction에게만 노출되는 진입점. 명시적 인터페이스 구현이라
    // 일반 참조로는 호출할 수 없어 protected 캡슐화가 유지된다.
    void IInteractionTarget.Interact()
    {
        Interact();
    }

    void IInteractionTarget.SetState(bool isOn)
    {
        SetState(isOn);
    }

    // 선택기에게만 노출되는 진입점. 인터랙션 입장에서는 선택이 곧 범위 안이다.
    bool IInteractionCandidate.IsEngaged => !ReferenceEquals(_interaction, null) && _interaction.IsEngaged;

    void IInteractionCandidate.SetSelected(bool isSelected)
    {
        if (IsSelected == isSelected)
        {
            return;
        }

        IsSelected = isSelected;
        OnSelectedChanged?.Invoke(isSelected);

        // 이미 꺼진 오브젝트는 Dispose가 나감 정리를 끝냈으므로 상태 값만 바꾼다.
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (isSelected)
        {
            EnterInteraction(_interactor);
        }
        else
        {
            ExitInteraction(_interactor);
        }
    }

    protected virtual void Interact()
    {
        OnInteract?.Invoke();
    }

    protected virtual void SetState(bool isOn)
    {
    }

    private void EnterInteraction(Collider2D other)
    {
        HandlePlayerEnter();
        _interaction?.OnTriggerEnter2D(other);
    }

    private void ExitInteraction(Collider2D other)
    {
        HandlePlayerExit();
        _interaction?.OnTriggerExit2D(other);
    }

    private void UnregisterFromSelector()
    {
        if (_selector == null)
        {
            return;
        }

        PlayerInteractTargetSelector selector = _selector;
        _selector = null;
        selector.Unregister(this);
    }
}
