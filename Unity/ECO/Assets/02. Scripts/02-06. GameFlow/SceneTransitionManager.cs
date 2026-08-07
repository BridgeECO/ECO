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

    [Foldout("Project")]
    [SerializeField]
    private bool _canLoadTitleOnStart = true;

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
            return FindPreloadedRegionScene().IsValid();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // 에디터에서 PersistentScene과 인게임 씬을 함께 열어 둔 채 플레이하는 경우를 여기서 처리한다.
        // Region.Start()가 플레이어를 텔레포트시키므로 Awake 단계에서 플레이어를 켜 둬야
        // PlayerMotor.Awake가 먼저 실행되어 Rigidbody2D 참조가 준비된다.
        AdoptPreloadedRegionScene();
    }

    private void Start()
    {
        // 이미 인게임 씬이 열려 있으면 타이틀을 띄우지 않고 그 씬으로 바로 플레이한다.
        if (!string.IsNullOrEmpty(_currentLoadedRegionScene))
        {
            // Awake 시점에는 아직 로드가 끝나지 않아 SetActiveScene이 실패하므로 여기서 옮긴다.
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_currentLoadedRegionScene));
            return;
        }

        if (_canLoadTitleOnStart)
        {
            LoadSceneAsync(ESceneNames.TitleScene).Forget();
        }
    }

    /// <summary>
    /// 플레이 시작 시 이미 열려 있는 인게임 씬을 현재 씬으로 채택한다.
    /// PersistentScene만 열고 플레이하면 채택할 씬이 없어 평소대로 타이틀이 로드된다.
    /// </summary>
    private void AdoptPreloadedRegionScene()
    {
        Scene preloadedScene = FindPreloadedRegionScene();
        if (!preloadedScene.IsValid())
        {
            return;
        }

        // 저장은 이 값을 ESceneNames로 변환해 기록하므로, 채택 시점에 채워 두지 않으면
        // 세이브 포인트를 밟아도 저장이 중단된다.
        _currentLoadedRegionScene = preloadedScene.name;

        if (_player != null)
        {
            _player.SetActive(true);
        }
        Debug.Log($"이미 열려 있는 '{preloadedScene.name}'에서 시작합니다. 타이틀 씬을 로드하지 않습니다.");
    }

    // PersistentScene과 타이틀을 제외한, 열려 있는 첫 인게임 씬.
    // 멀티 씬 진입 직후에는 아직 isLoaded가 false이므로 IsValid로만 판정한다.
    private static Scene FindPreloadedRegionScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !IsRegionSceneName(scene.name))
            {
                continue;
            }
            return scene;
        }
        return default;
    }

    private static bool IsRegionSceneName(string sceneName)
    {
        return sceneName != ESceneNames.PersistentScene.ToString()
            && sceneName != ESceneNames.TitleScene.ToString();
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
            PlayTransitionBgm(targetSceneName);

            await UIManager.Instance.FadeOutAsync(1f, this.GetCancellationTokenOnDestroy());

            await LoadSceneAsync(targetSceneName);

            await UIManager.Instance.FadeInAsync(1f, this.GetCancellationTokenOnDestroy());
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

    // 전환 대상 씬에 따라 적절한 BGM으로 크로스페이드한다.
    // Title 씬으로 복귀할 때는 Title BGM, 인게임 씬 진입 시 SyrNormal BGM으로 전환한다.
    private void PlayTransitionBgm(ESceneNames targetSceneName)
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }

        if (targetSceneName == ESceneNames.TitleScene)
        {
            SoundManager.Instance.PlayBgm(EBgmType.Title);
            return;
        }

        SoundManager.Instance.PlayBgm(EBgmType.SyrNormal);
    }
}