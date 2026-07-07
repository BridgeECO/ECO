using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class SceneTransitionManager : MonoBehaviourSingleton<SceneTransitionManager>
{
    private string _currentLoadedRegionScene;
    private bool _isTransitioning;
    
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _player;

    public string CurrentLoadedRegionScene => _currentLoadedRegionScene;
    public bool IsTransitioning => _isTransitioning;
    public bool IsGameplayScene
    {
        get
        {
            if (!string.IsNullOrEmpty(_currentLoadedRegionScene))
            {
                return _currentLoadedRegionScene != ESceneNames.TitleScene.ToString();
            }

            // 에디터 멀티 씬 테스트 시 PersistentScene이 Active Scene으로 지정되어 있어도
            // 로드된 씬 중 타이틀이나 PersistentScene이 아닌 인게임 씬이 존재하면 게임플레이 상태로 판정한다.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    string name = scene.name;
                    if (name != ESceneNames.TitleScene.ToString() && name != "PersistentScene")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

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

        if (_player != null)
        {
            _player.SetActive(IsGameplayScene);
        }

        await UniTask.Yield();
    }

    public async UniTask TransitionToNewRegionAsync(ESceneNames targetSceneName)
    {
        if (_isTransitioning)
        {
            return;
        }
        _isTransitioning = true;
        InputHandler.BlockInput();

        Time.timeScale = 1f;
        if (UIManager.Instance != null && UIManager.Instance.PopupHandler != null)
        {
            UIManager.Instance.PopupHandler.ClearAllPopups();
        }

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

    public async UniTask TransitionToTitleAsync()
    {
        await TransitionToNewRegionAsync(ESceneNames.TitleScene);
    }
}