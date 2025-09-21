
namespace ECO
{
    public class App
    {
        private static App _inst = null;

        public static App Inst()
        {
            if (_inst == null)
                _inst = new App();

            return _inst;
        }

        public static void Destroy()
        {
            _inst = null;
        }

        public bool Create()
        {
            return true;
        }
    }
}