public class POCOSingleton<T> where T : class, new()
{
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
