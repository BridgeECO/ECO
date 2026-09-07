using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 씬에 놓는 컷씬 한 편. 기획자가 조립하는 유일한 대상이다.
///
/// 스텝 리스트만 직접 소유하고 실행·취소·소유권 반납은 CutsceneSession에 위임한다.
/// 리스트를 세션 쪽에 두면 인스펙터가 Director > Session > Steps로 불필요하게 깊어진다.
/// </summary>
public class CutsceneDirector : MonoBehaviour, IResettable
{
    [Foldout("Settings")]
    [SerializeField]
    [Tooltip("NPC 이벤트가 이 컷씬을 지목할 때 쓰는 이름입니다. 씬 안에서 겹치면 안 됩니다.")]
    private string _cutsceneId = string.Empty;

    [SerializeField]
    [Tooltip("체크하면 한 번 재생한 뒤 다시 재생하지 않습니다.")]
    private bool _isOneShot = true;

    [SerializeField]
    [Tooltip("체크하면 ESC로 건너뛸 수 있습니다.")]
    private bool _isSkippable = true;

    [SerializeField]
    [Tooltip("재생하는 동안 플레이어 입력을 막습니다.")]
    private bool _isBlockInput = true;

    [SerializeField]
    [Tooltip("재생하는 동안 플레이어를 제자리에 세웁니다. 입력 차단만으로는 대시와 중력이 남습니다.")]
    private bool _isFreezePlayer = true;

    // 판정과 실행은 세션이 맡지만 리스트는 여기서 소유한다.
    [SerializeReference]
    private List<CutsceneStepBase> _steps = new List<CutsceneStepBase>();

    [Foldout("Hierarchy")]
    [SerializeField]
    [Tooltip("ESC로 건너뛸 수 있음을 알리는 오브젝트입니다. 비워 두면 안내를 띄우지 않습니다.")]
    private GameObject _skipHintObject;

    [SerializeField]
    [Tooltip("리스폰으로 되돌릴 때 꺼야 할 연출 오브젝트입니다. 순수 SpriteRenderer는 방 리셋이 되돌려 주지 않습니다.")]
    private List<GameObject> _resetDeactivateTargets = new List<GameObject>();

    private readonly CutsceneEventBridge _eventBridge = new CutsceneEventBridge();

    private CutsceneRuntime _runtime;

    public string CutsceneId => _cutsceneId;
    public IReadOnlyList<CutsceneStepBase> Steps => _steps;
    public bool IsSkippable => _isSkippable;
    public bool IsBlockInput => _isBlockInput;
    public bool IsFreezePlayer => _isFreezePlayer;
    public GameObject SkipHintObject => _skipHintObject;

    public bool CanPlay => !Runtime.Session.IsPlaying && (!_isOneShot || !Runtime.Progress.IsFired);

    // 비활성 상태로 배치된 오브젝트도 Room.ResetRoom의 대상이 되어 Awake 없이 ResetState가
    // 먼저 올 수 있다. 그래서 첫 접근 시점에 만든다.
    private CutsceneRuntime Runtime
    {
        get
        {
            if (ReferenceEquals(_runtime, null))
            {
                _runtime = new CutsceneRuntime(transform);
            }

            return _runtime;
        }
    }

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        _eventBridge.OnAbortRequested += Abort;
        _eventBridge.OnSaveSnapshotRequested += SaveProgress;
        _eventBridge.TryAddListeners();
    }

    // EventManager가 아직 없는 로드 순서에서도 구독이 성사되도록 한 번 더 시도한다.
    private void Start()
    {
        _eventBridge.TryAddListeners();
    }

    private void OnDisable()
    {
        _eventBridge.RemoveListeners();
        _eventBridge.OnAbortRequested -= Abort;
        _eventBridge.OnSaveSnapshotRequested -= SaveProgress;

        Runtime.Session.Abort(ECutsceneAbortReason.StopInPlace);
        Runtime.Session.ForceRelease();
    }

#if UNITY_EDITOR
    // 수리기가 원소를 갈아 끼워야 해서 쓰기 가능한 목록이 필요하다.
    internal List<CutsceneStepBase> EditableSteps => _steps;

    private void OnValidate()
    {
        CutsceneStepValidator.Validate(this);
        CutsceneFlowValidator.Validate(this);
        CutsceneReferenceRepairer.Repair(this);
    }
#endif
    #endregion

    #region Logic
    public void Play()
    {
        PlayAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public async UniTask PlayAsync(CancellationToken cancellationToken)
    {
        if (!CanPlay)
        {
            return;
        }

        // 트리거와 NPC 두 진입점이 같은 판정을 공유하도록 여기서 한 번만 기록한다.
        Runtime.Progress.MarkFired();
        await Runtime.Session.PlayAsync(this, cancellationToken);
    }

    public void RequestSkip()
    {
        Runtime.Session.RequestSkip();
    }

    public void Abort(ECutsceneAbortReason reason)
    {
        Runtime.Session.Abort(reason);
    }

    public void BindPlayer(PlayerStateMachine player)
    {
        Runtime.Context.BindPlayer(player);
    }

    public void BindPlayerFrom(Collider2D playerCollider)
    {
        Runtime.Context.BindPlayerFrom(playerCollider);
    }

    public void ResetState()
    {
        Runtime.Session.Abort(ECutsceneAbortReason.WorldWillReset);
        Runtime.Session.ForceRelease();
        Runtime.Progress.ResetToSavedState();
        DeactivateResetTargets();
    }

    private void SaveProgress()
    {
        Runtime.Progress.SaveFiredState();
    }

    private void DeactivateResetTargets()
    {
        for (int i = 0; i < _resetDeactivateTargets.Count; i++)
        {
            GameObject target = _resetDeactivateTargets[i];
            if (target != null)
            {
                target.SetActive(false);
            }
        }
    }
    #endregion

    #region Preview
    [Foldout("Preview")]
    [Button("컷씬 재생")]
    private void PlayPreview()
    {
        CutscenePreview.Play(this, Runtime);
    }

    [Button("끝 상태로 건너뛰기")]
    private void ApplyFinalStatesPreview()
    {
        CutscenePreview.ApplyFinalStates(_steps, Runtime);
    }

    [Button("정지 및 소유권 반환")]
    private void AbortPreview()
    {
        CutscenePreview.Abort(Runtime);
    }
    #endregion
}
