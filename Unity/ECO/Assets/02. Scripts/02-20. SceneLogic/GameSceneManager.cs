using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviourSingleton<GameSceneManager>
{
    private string _currentLoadedRegionScene;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        LoadRegionSceneAsync(nameof(ESceneNames.Region1Scene)).Forget();
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