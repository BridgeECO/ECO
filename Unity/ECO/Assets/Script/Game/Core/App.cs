


namespace ECO
{
    public class App
    {
        public InputSystem InputSys { get; private set; } = new InputSystem();
        public ConfigSystem CfgSys { get; private set; } = new ConfigSystem();
        public TickerManager TickerMgr { get; private set; } = new TickerManager();


        public void Destroy()
        {
            InputSys.Destroy();
            CfgSys.Destroy();
        }

        public bool Create()
        {
            if (!CfgSys.Create())
                return false;

            return true;
        }

        public void Update()
        {
            InputSys.Update();
            TickerMgr.Update();
        }
    }
}