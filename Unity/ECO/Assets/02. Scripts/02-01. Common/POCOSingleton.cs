public class POCOSingleton<T> where T : class, new()
{
    protected bool _dontDestroy;
    protected bool _lazyInitialization;

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (ReferenceEquals(_instance, null))
            {
                _instance = new T();
            }
            return _instance;
        }
    }
}
