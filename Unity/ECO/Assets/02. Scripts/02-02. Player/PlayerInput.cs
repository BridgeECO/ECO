using UnityEngine;
using System;

public class PlayerInput : MonoBehaviour
{
    public Action OnJumpPressed;
    public Action OnJumpReleased;
    public Action OnDashPressed;
    public Action OnDashReleased;

    public float HorizontalInput { get; private set; }
    public Vector2 MouseWorldPosition { get; private set; }

    private Camera _mainCamera;

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
        if (Input.GetMouseButtonDown(0))
        {
            OnDashPressed?.Invoke();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnDashReleased?.Invoke();
        }
    }
}