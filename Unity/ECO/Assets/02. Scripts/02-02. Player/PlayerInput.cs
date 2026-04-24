using System;
using UnityEngine;
using VInspector;

public class PlayerInput : MonoBehaviour
{
    public Action OnJumpPressed;
    public Action OnJumpReleased;
    public Action OnDashPressed;
    public Action OnDashReleased;

    public float HorizontalInput { get; private set; }
    public Vector2 MouseWorldPosition { get; private set; }

    [field: SerializeField]
    public bool IsDashLocked { get; set; }

    [field: SerializeField]
    public bool IsWallSlideLocked { get; set; }

    private Camera _mainCamera;

    #region Unity Lifecycle Methods
    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (InputHandler.IsInputBlocked)
        {
            HorizontalInput = 0f;
            return;
        }

        HorizontalInput = Input.GetAxisRaw("Horizontal");
        MouseWorldPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Jump();
        Dash();
    }
    #endregion

    #region Manipulations(Jump & Dash)
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpPressed?.Invoke();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnJumpReleased?.Invoke();
        }
    }

    private void Dash()
    {
        if (IsDashLocked)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            OnDashPressed?.Invoke();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnDashReleased?.Invoke();
        }
    }
    #endregion

    #region Debug Buttons
    [Button("Unlock Dash"), ShowIf("IsDashLocked")]
    private void UnlockDash()
    {
        IsDashLocked = false;
    }

    [Button("Lock Dash"), HideIf("IsDashLocked")]
    private void LockDash()
    {
        IsDashLocked = true;
    }

    [Button("Unlock Wall Slide"), ShowIf("IsWallSlideLocked")]
    private void UnlockWallSlide()
    {
        IsWallSlideLocked = false;
    }

    [Button("Lock Wall Slide"), HideIf("IsWallSlideLocked")]
    private void LockWallSlide()
    {
        IsWallSlideLocked = true;
    }
    #endregion
}
