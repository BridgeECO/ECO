using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class SceneTransitionManager : MonoBehaviourSingleton<SceneTransitionManager>
{
    private string _currentLoadedRegionScene;

    private bool _isTransitioning;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        LoadSceneAsync(ESceneNames.TitleScene).Forget();
    }

    private async UniTask LoadSceneAsync(ESceneNames eSceneName)
    {
        string sceneName = eSceneName.ToString();
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
            Debug.LogError($"'{sceneName}' 로드 실패. 빌드 프로필 확인해보세요");
            return;
        }

        await loadOp;

        Scene newlyLoadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newlyLoadedScene);
        _currentLoadedRegionScene = sceneName;
    }

    public async UniTask TransitionToNewRegionAsync(ESceneNames targetSceneName)
    {
        if (_isTransitioning)
        {
            return;
        }
        _isTransitioning = true;
        InputHandler.BlockInput();

        try
        {
            var fadeOutUcs = new UniTaskCompletionSource();
            UIManager.Instance.FadeInLoadingPanel(() => fadeOutUcs.TrySetResult());
            await fadeOutUcs.Task;

            await LoadSceneAsync(targetSceneName);

            var fadeInUcs = new UniTaskCompletionSource();
            UIManager.Instance.FadeOutLoadingPanel(() => fadeInUcs.TrySetResult());
            await fadeInUcs.Task;
        }
        finally
        {
            _isTransitioning = false;
            InputHandler.UnblockInput();
        }
    }

    public async UniTask TransitionToLobbyAsync()
    {
        // await TransitionToNewRegionAsync(nameof(ESceneNames.LobbyScene));
    }
}