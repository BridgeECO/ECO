using System;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Collider2D))]
public abstract class SpecialObjectBase : MonoBehaviour, IRoomResettable
{
    public Action OnInteract;

    [Foldout("Project")]
    [SerializeField]
    protected EInteractionType _interactionType;

    public EInteractionType InteractionType { get => _interactionType; private set => _interactionType = value; }

    public bool IsPlayerInRange { get; private set; }

    private InteractionBase _interaction;

    protected virtual void Awake()
    {
        switch (_interactionType)
        {
            case EInteractionType.Button:
                _interaction = new ButtonInteraction(this);
                break;
            case EInteractionType.AutoPlay:
                _interaction = new AutoPlayInteraction(this);
                break;
            case EInteractionType.PressurePlate:
                _interaction = new PressurePlateInteraction(this);
                break;
        }
    }

    protected virtual void OnDisable()
    {
        _interaction?.Dispose();
        IsPlayerInRange = false;
    }

    public virtual void ResetState()
    {
        IsPlayerInRange = false;
        if (_interactionType == EInteractionType.Button)
        {
            // Optional: Button interaction doesn't inherently store persistent state here, 
            // but we can add more reset logic for interaction later if needed.
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            return;
        }

        IsPlayerInRange = true;
        HandlePlayerEnter();
        _interaction?.OnTriggerEnter2D(other);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
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

    public void CallInteract()
    {
        Interact();
    }

    public void CallSetState(bool isOn)
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
