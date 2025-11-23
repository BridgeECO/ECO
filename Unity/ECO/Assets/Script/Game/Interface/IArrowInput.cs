namespace ECO
{
    public interface IArrowInput : IInput
    {
        public void InputRightKeyDown();
        public void InputLeftKeyDown();
        public void InputUpKeyDown();
        public void InputDownKeyDown();

        public void InputRightKey();
        public void InputLeftKey();
        public void InputUpKey();
        public void InputDownKey();

        public void InputRightKeyUp();
        public void InputLeftKeyUp();
        public void InputUpKeyUp();
        public void InputDownKeyUp();
    }
}