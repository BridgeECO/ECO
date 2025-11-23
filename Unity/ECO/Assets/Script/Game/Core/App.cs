


namespace ECO
{
    public class App
    {
        public InputSystem InputSys { get; private set; } = new InputSystem();

        public void Destroy()
        {
            InputSys.Destroy();
        }

        public bool Create()
        {
            return true;
        }

        public void Update()
        {
            InputSys.Update();
        }
    }
}