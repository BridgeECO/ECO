using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 키보드 UI 입력을 일원화하는 싱글톤 매니저.
/// IKeyboardControllable 핸들러를 Stack으로 관리하며, 항상 최상단 핸들러에만 입력을 위임한다.
/// 팬업 등 새로운 UI가 활성화될 때 PushHandler, 비활성화될 때 PopHandler를 호출한다.
/// </summary>
public class UI_KeyboardInputManager : MonoBehaviourSingleton<UI_KeyboardInputManager>
{
    private readonly List<IKeyboardControllable> _handlers = new List<IKeyboardControllable>();

    #region Unity Lifecycle Methods
    private void Update()
    {
        DispatchInput();
    }
    #endregion

    #region Handler Stack Management
    public void PushHandler(IKeyboardControllable handler)
    {
        if (handler == null)
        {
            return;
        }

        PruneDestroyedHandlers();

        if (!_handlers.Contains(handler))
        {
            _handlers.Add(handler);
        }
    }

    public void PopHandler(IKeyboardControllable handler = null)
    {
        PruneDestroyedHandlers();

        if (handler != null)
        {
            _handlers.Remove(handler);
        }
        else if (_handlers.Count > 0)
        {
            _handlers.RemoveAt(_handlers.Count - 1);
        }
    }
    #endregion

    #region Input Dispatch
    private void DispatchInput()
    {
        if (_handlers.Count == 0)
        {
            return;
        }

        // 키 입력이 없는 프레임에는 가짜 null 체크 오버헤드를 방지하기 위해 얼리 리턴 처리합니다. (컨벤션 1-d 준수)
        if (!Input.anyKeyDown)
        {
            return;
        }

        PruneDestroyedHandlers();

        if (_handlers.Count == 0)
        {
            return;
        }

        IKeyboardControllable handler = _handlers[_handlers.Count - 1];

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            handler.OnMoveUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            handler.OnMoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            handler.OnMoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            handler.OnMoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            handler.OnConfirm();
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// 파괴된 유니티 오브젝트 핸들러를 감지하여 리스트에서 제거한다.
    /// 인터페이스 타입은 직접 null 비교 시 유니티의 가짜 null(Fake null) 연산자가 작동하지 않으므로,
    /// UnityEngine.Object로 캐스팅하여 파괴 여부를 판별한다.
    /// </summary>
    private void PruneDestroyedHandlers()
    {
        for (int i = _handlers.Count - 1; i >= 0; i--)
        {
            if (_handlers[i] == null || (_handlers[i] is UnityEngine.Object unityObject && unityObject == null))
            {
                _handlers.RemoveAt(i);
            }
        }
    }
    #endregion
}
