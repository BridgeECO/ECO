using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    private string _currentLoadedRegionScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadRegionSceneAsync("Region1Scene").Forget();
    }

    private async UniTask LoadRegionSceneAsync(string sceneName)
    {
        if (!string.IsNullOrEmpty(_currentLoadedRegionScene))
        {
            await SceneManager.UnloadSceneAsync(_currentLoadedRegionScene);
        }

        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        Scene newlyLoadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newlyLoadedScene);
        _currentLoadedRegionScene = sceneName;
    }

    public async UniTask TransitionToNewRegionAsync(string targetSceneName)
    {
        await LoadRegionSceneAsync(targetSceneName);

        //플레이어의 위치를 정하는 코드가 추가로 필요함
    }
}
