using UnityEngine;
using UnityEngine.Events;

namespace ECO
{
    public class InputKeyEvent
    {
        public InputKeyEvent(KeyCode keyCode, UnityAction onKeyDownEvent = null, UnityAction onKeyEvent = null, UnityAction onKeyUpEvent = null)
        {
            KeyCode = keyCode;
            _onKeyDownEvent = onKeyDownEvent;
            _onKeyEvent = onKeyEvent;
            _onKeyUpEvent = onKeyUpEvent;
        }

        public KeyCode KeyCode { get; private set; }

        private UnityAction _onKeyDownEvent;
        private UnityAction _onKeyEvent;
        private UnityAction _onKeyUpEvent;

        //public void InputKey()
        //{
        //    if (InputAction.getk)
        //}
    }
}