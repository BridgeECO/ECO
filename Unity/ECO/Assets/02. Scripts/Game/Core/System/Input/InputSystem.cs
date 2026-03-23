using System.Collections.Generic;

namespace ECO
{
    public class InputSystem : IDestroyable
    {
        private List<InputKeyEvent> _keyEvtList = new List<InputKeyEvent>();

        public void Destroy()
        {
            _keyEvtList.Clear();
        }

        public void Update()
        {
            foreach (var evt in _keyEvtList)
                evt.InputKey();
        }

        public void RegisterEvt(InputKeyEvent evt)
        {
            _keyEvtList.Add(evt);
        }
    }
}