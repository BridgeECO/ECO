using System.Collections.Generic;
using UnityEngine;
using VInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerFSMDebugger : MonoBehaviour
{
    private PlayerStateMachine _stateMachine;

    [Foldout("Debug")]
    [SerializeField, ReadOnly]
    private string _currentState;

    [SerializeField, ReadOnly]
    private string _transitionHistory;

    [Foldout("Timers")]
    [SerializeField, ReadOnly]
    private float _jumpBufferTimer;

    [SerializeField, ReadOnly]
    private float _coyoteTimer;

    [SerializeField, ReadOnly]
    private float _dashCooldownTimer;

    [SerializeField, ReadOnly]
    private float _inputLockTimer;

    private Queue<EPlayerState> _histories = new Queue<EPlayerState>();

    private void Awake()
    {
        _stateMachine = GetComponent<PlayerStateMachine>();
    }

    private void OnEnable()
    {
        _stateMachine.OnStateChanged += HandleStateChanged;
    }

    private void Update()
    {
        _jumpBufferTimer = _stateMachine.JumpBufferTimer;
        _coyoteTimer = _stateMachine.CoyoteTimer;
        _dashCooldownTimer = _stateMachine.DashCooldownTimer;
        _inputLockTimer = _stateMachine.InputLockTimer;
    }

    private void OnDisable()
    {
        _stateMachine.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(EPlayerState newState)
    {
        _currentState = newState.ToString();
        _histories.Enqueue(newState);

        if (4 < _histories.Count)
        {
            _histories.Dequeue();
        }

        _transitionHistory = string.Join(" -> ", _histories);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Handles.Label(transform.position + Vector3.up * 1.5f, _currentState);
        }
    }
#endif
}
