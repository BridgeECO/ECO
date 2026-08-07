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
    public float VerticalInput { get; private set; }
    public Vector2 MouseWorldPosition { get; private set; }

    [field: SerializeField]
    public bool IsDashLocked { get; private set; }

    [field: SerializeField]
    public bool IsWallSlideLocked { get; private set; }

    private Camera _mainCamera;
    // 매 프레임 SceneTransitionManager를 조회하지 않도록 이벤트 통지로 캐싱한다.
    // 매니저가 없는 테스트 씬에서도 입력이 동작해야 하므로 true로 시작한다.
    // (플레이어 오브젝트 자체가 타이틀에서는 비활성이라 게임플레이 밖 오동작은 없다)
    private bool _isGameplayScene = true;

    #region Unity Lifecycle Methods
    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EventManager.Instance.AddEventListener<bool>(EEventType.GameplaySceneChanged, SetGameplayScene);
    }

    private void Start()
    {
        // 구독(OnEnable) 이전에 확정된 상태(에디터 멀티 씬 채택 등)를 한 번 동기화한다.
        if (SceneTransitionManager.HasInstance)
        {
            _isGameplayScene = SceneTransitionManager.Instance.IsGameplayScene;
        }
    }

    private void Update()
    {
        if (InputHandler.IsInputBlocked || !_isGameplayScene)
        {
            HorizontalInput = 0f;
            return;
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        UpdateHorizontalInput();
        UpdateVerticalInput();
        UpdateMousePosition();
        Jump();
        Dash();
    }

    private void OnDisable()
    {
        RemoveEventListeners();
    }
    #endregion

    private void RemoveEventListeners()
    {
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener<bool>(EEventType.GameplaySceneChanged, SetGameplayScene);
        }
    }

    private void SetGameplayScene(bool isGameplayScene)
    {
        _isGameplayScene = isGameplayScene;
    }

    #region Input Processings
    private void UpdateHorizontalInput()
    {
        HorizontalInput = Input.GetAxisRaw("Horizontal");
    }

    private void UpdateVerticalInput()
    {
        VerticalInput = Input.GetAxisRaw("Vertical");
    }

    private void UpdateMousePosition()
    {
        if (_mainCamera == null)
        {
            return;
        }

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
        MouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
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

    /// <summary>
    /// 잠긴 능력을 해제한다. 외부(NPC 이벤트 등)가 잠금 상태를 직접 대입하는 대신
    /// 의도가 드러나는 이 메서드를 통해서만 해제할 수 있다.
    /// </summary>
    public void UnlockAbility(EPlayerUnlockableAbility abilityType)
    {
        switch (abilityType)
        {
            case EPlayerUnlockableAbility.Dash:
                IsDashLocked = false;
                break;
            case EPlayerUnlockableAbility.WallSlide:
                IsWallSlideLocked = false;
                break;
        }
    }

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
