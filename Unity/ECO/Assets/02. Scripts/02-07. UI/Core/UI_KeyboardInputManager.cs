using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 키보드 UI 입력을 일원화하는 싱글톤 매니저.
/// IKeyboardControllable 핸들러를 Stack으로 관리하며, 항상 최상단 핸들러에만 입력을 위임한다.
/// 팬업 등 새로운 UI가 활성화될 때 PushHandler, 비활성화될 때 PopHandler를 호출한다.
/// </summary>
public class UI_KeyboardInputManager : MonoBehaviourSingleton<UI_KeyboardInputManager>
{
    private readonly Stack<IKeyboardControllable> _handlers = new Stack<IKeyboardControllable>();

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        DispatchInput();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        _handlers.Clear();
    }
    #endregion

    #region Handler Stack Management
    public void PushHandler(IKeyboardControllable handler)
    {
        _handlers.Push(handler);
    }

    public void PopHandler()
    {
        if (0< _handlers.Count )
        {
            _handlers.Pop();
        }
    }
    #endregion

    #region Input Dispatch
    private void DispatchInput()
    {
        if (!_handlers.TryPeek(out IKeyboardControllable handler))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            handler.OnMoveUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            handler.OnMoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            handler.OnMoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            handler.OnMoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            handler.OnConfirm();
        }
    }
    #endregion
}
