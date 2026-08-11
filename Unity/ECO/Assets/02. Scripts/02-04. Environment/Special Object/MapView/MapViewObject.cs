using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 상호작용 시 카메라를 지정한 지점으로 옮겨 넓은 구역을 한눈에 보여주는 오브젝트.
/// 범위를 벗어나거나 상호작용 키를 다시 누르면 카메라가 플레이어에게 돌아온다.
/// 스위치와 한 오브젝트에 함께 부착할 수 있으며, 이때는 양쪽 InteractionType을 모두
/// ButtonToggle로 맞춰야 한 번의 입력에 두 동작의 켜짐/꺼짐 시점이 일치한다.
/// </summary>
public class MapViewObject : SpecialObjectBase
{
    // 조망은 평소보다 넓게 보여주는 것이 목적이므로, 메인 카메라의 기본 Z(-10)보다 앞으로는 나올 수 없다.
    private const float MAX_CAMERA_Z = -10f;
    private const float MIN_CAMERA_Z = -30f;

    // 베이스가 "Project"를 열어둔 탓에 이 값들을 같은 이름으로 묶으면 인스펙터에서 사라진다.
    // VInspector의 mergeFoldouts가 같은 이름 그룹을 베이스 쪽으로 끌어올리는데, 그 자리가
    // _buttonOneShotDuration의 [ShowIf] 숨김 구간 안이기 때문이다(ShowIf는 타입 경계 마커까지 유지된다).
    // 조정용 수치라 참조 할당 위치와도 무관하므로 CameraController와 같은 "Settings" 그룹을 쓴다.
    [Foldout("Settings")]
    [Header("View")]
    [SerializeField]
    [Range(MIN_CAMERA_Z, MAX_CAMERA_Z)]
    [Tooltip("조망 시 카메라가 놓일 Z 좌표입니다. 뒤로 물러날수록(값이 작을수록) 넓은 범위가 보입니다. " +
             "-10이 평소 게임 화면과 같은 크기(최댓값)이고, -20으로 두면 약 2배 넓게 보입니다.")]
    private float _cameraZ = MAX_CAMERA_Z;

    [Header("Transition")]
    [SerializeField]
    [Min(0f)]
    [Tooltip("조망을 시작할 때 카메라가 목표 위치와 크기까지 이동하는 시간(초)입니다. 짧을수록 빠르게 전환됩니다.")]
    private float _enterDuration = 0.6f;

    [SerializeField]
    [Min(0f)]
    [Tooltip("조망을 끝내고 카메라가 플레이어에게 돌아오는 시간(초)입니다.")]
    private float _exitDuration = 0.6f;

    [Foldout("Hierarchy")]
    [SerializeField]
    [Tooltip("카메라가 비출 위치. 자식 오브젝트 ViewPoint_Camera를 씬 뷰에서 직접 드래그해 지정합니다. " +
             "XY만 사용하며 Z는 위 Camera Z 값이 결정합니다.")]
    private Transform _viewPoint;

    private MapViewCamera _mapViewCamera;
    private CancellationTokenSource _viewCts;

    protected override void Awake()
    {
        base.Awake();
        _mapViewCamera = new MapViewCamera();
    }

    private void OnEnable()
    {
        if (EventManager.Instance == null)
        {
            return;
        }
        EventManager.Instance.AddEventListener(EEventType.RoomChanged, OnRoomChanged);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_viewPoint == null)
        {
            return;
        }

        MapViewGizmoDrawer.DrawViewFrame(transform.position, _viewPoint.position, _cameraZ);
    }
#endif

    protected override void OnDisable()
    {
        // base가 Interaction을 Dispose하며 SetState(false)를 거쳐 StopView를 유발하므로,
        // 비활성화 시점에 보간이 남지 않도록 먼저 즉시 종료해 둔다.
        StopViewImmediate();
        base.OnDisable();
        RemoveEventListeners();
    }

    public override void ResetState()
    {
        base.ResetState();
        StopViewImmediate();
    }

    protected override void Interact()
    {
        base.Interact();
        StartView();
    }

    protected override void SetState(bool isOn)
    {
        base.SetState(isOn);
        if (!isOn)
        {
            StopView();
        }
    }

    private void StartView()
    {
        if (_viewPoint == null)
        {
            Debug.LogError($"[MapViewObject] 조망 지점(_viewPoint)이 인스펙터에 할당되지 않았습니다. ({name})");
            return;
        }

        if (!_mapViewCamera.TryBind())
        {
            return;
        }

        RestartViewCts();
        EnterViewAsync(_viewCts.Token).Forget();
    }

    private void StopView()
    {
        if (!_mapViewCamera.IsViewing)
        {
            return;
        }

        RestartViewCts();
        ExitViewAsync(_viewCts.Token).Forget();
    }

    private void StopViewImmediate()
    {
        CancelViewCts();
        _mapViewCamera?.StopImmediate();
    }

    private async UniTaskVoid EnterViewAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _mapViewCamera.EnterAsync(_viewPoint.position, _cameraZ, _enterDuration, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTaskVoid ExitViewAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _mapViewCamera.ExitAsync(_exitDuration, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void RestartViewCts()
    {
        CancelViewCts();
        _viewCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }

    private void CancelViewCts()
    {
        if (_viewCts is null)
        {
            return;
        }

        _viewCts.Cancel();
        _viewCts.Dispose();
        _viewCts = null;
    }

    private void OnRoomChanged()
    {
        CancelViewCts();
        _mapViewCamera?.AbandonForRoomTransition();
    }

    private void RemoveEventListeners()
    {
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.RoomChanged, OnRoomChanged);
        }
    }
}
