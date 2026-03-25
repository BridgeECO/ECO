using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField]
    private bool _isDontDestroyOnLoad;

    [SerializeField]
    private bool _lazyInitialization;

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (!_lazyInitialization)
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (_isDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
