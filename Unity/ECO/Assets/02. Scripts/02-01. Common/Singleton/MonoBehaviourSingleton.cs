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

    // Instance getter의 Find 폴백과 판정 기준을 일치시키기 위해 동일한 폴백을 수행한다.
    // (Awake 이전이나 lazyInitialization 상태에서도 씬에 존재하면 true)
    public static bool HasInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
            }
            return _instance != null;
        }
    }

    protected virtual void Awake()
    {
        if (!_lazyInitialization)
        {
            if (_instance == null || _instance == this)
            {
                _instance = this as T;
                if (_isDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(transform.root.gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
