using ECO;
using UnityEngine;
using UnityEngine.Events;

public class InputSystem
{
    private IArrowInput _arrowInput = null;

    public void Update()
    {
        InputArrow();
    }

    private void InputArrow()
    {
        if (_arrowInput == null)
            return;

        ExecuteKeyAndDownInput(KeyCode.RightArrow, _arrowInput.InputRightKeyDown);
    }

    private bool ExecuteKeyAndDownInput(KeyCode keyCode, UnityAction onInputKey)
    {
        //Down 먼저 처리함
        if (Input.GetKeyDown(keyCode))
        {
            onInputKey.Invoke();
            return true;
        }

        if (Input.GetKey(keyCode))
        {
            onInputKey.Invoke();
            return true;
        }

        return false;
    }

    private bool ExecuteKeyUpInput(KeyCode keyCode, UnityAction onInputKey)
    {
        //Down 먼저 처리함
        if (Input.GetKeyUp(keyCode))
        {
            onInputKey.Invoke();
            return true;
        }

        return false;
    }
}
