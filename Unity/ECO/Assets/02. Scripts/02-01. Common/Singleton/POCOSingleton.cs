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

    /// <summary>
    /// 인스턴스를 폐기해 다음 접근 시 새로 생성되게 한다.
    /// 도메인 리로드 비활성화(Enter Play Mode Options) 환경에서 플레이 세션 간
    /// static 상태 잔류를 막기 위해 파생 클래스의 리셋 훅에서 호출한다.
    /// </summary>
    protected static void ResetInstance()
    {
        _instance = null;
    }
}
