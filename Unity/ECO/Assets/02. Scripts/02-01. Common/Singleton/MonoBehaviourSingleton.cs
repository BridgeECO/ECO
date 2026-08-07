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

    // 매 프레임 호출되는 가드(오디오 감쇠 등)에서도 쓰이므로 씬 탐색 없이 캐시만 확인한다.
    // 해제부 가드가 주 용도라, 이 시점에는 Instance 접근으로 이미 캐시가 채워져 있다.
    public static bool HasInstance => _instance != null;

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
