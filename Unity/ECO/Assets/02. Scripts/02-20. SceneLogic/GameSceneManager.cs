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
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(_currentLoadedRegionScene);
            if (unloadOp != null)
            {
                await unloadOp;
            }
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (loadOp == null)
        {
            Debug.LogError($"Scene '{sceneName}' could not be loaded. Check Build Settings and spelling.");
            return;
        }

        await loadOp;

        Scene newlyLoadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newlyLoadedScene);
        _currentLoadedRegionScene = sceneName;
    }

    public async UniTask TransitionToNewRegionAsync(string targetSceneName)
    {
        await LoadRegionSceneAsync(targetSceneName);

    }
}