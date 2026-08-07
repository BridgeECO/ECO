using System;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Collider2D))]
public abstract class SpecialObjectBase : MonoBehaviour, IResettable, IInteractionTarget
{
    public Action OnInteract;

    [Foldout("Project")]
    [SerializeField]
    protected EInteractionType _interactionType;

    [Foldout("Project")]
    [ShowIf(nameof(_interactionType), EInteractionType.ButtonOneShotTimer)]
    [SerializeField]
    private float _buttonOneShotDuration = 3f;

    public EInteractionType InteractionType { get => _interactionType; private set => _interactionType = value; }

    public bool IsPlayerInRange { get; private set; }

    private InteractionBase _interaction;

    protected virtual void Awake()
    {
        // 파생 Interaction 구체 타입 선택은 팩토리가 담당한다. (베이스→파생 의존 제거)
        _interaction = InteractionFactory.Create(_interactionType, this, _buttonOneShotDuration);
    }

    protected virtual void OnDisable()
    {
        _interaction?.Dispose();
        IsPlayerInRange = false;
    }

    public virtual void ResetState()
    {
        IsPlayerInRange = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        string validTag = _interaction != null ? _interaction.ValidTag : nameof(ETags.PlayerInteract);
        if (!other.CompareTag(validTag))
        {
            return;
        }

        IsPlayerInRange = true;
        HandlePlayerEnter();
        _interaction?.OnTriggerEnter2D(other);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        string validTag = _interaction != null ? _interaction.ValidTag : nameof(ETags.PlayerInteract);
        if (!other.CompareTag(validTag))
        {
            return;
        }

        IsPlayerInRange = false;
        HandlePlayerExit();
        _interaction?.OnTriggerExit2D(other);
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

    protected virtual void Interact()
    {
        OnInteract?.Invoke();
    }

    protected virtual void SetState(bool isOn)
    {
    }
}
