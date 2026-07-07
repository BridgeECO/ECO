using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

// 에너지 라인 트래킹 시스템의 유일한 MonoBehaviour 진입점.
// EnergySwitch가 발행하는 정적 이벤트를 구독하여 시퀀스를 시작하며,
// 순수 C# 클래스(CameraLogic, FadeLogic)를 소유하고 LateUpdate에서 구동한다.
public class EnergyLineTracker : MonoBehaviour
{
    // EnergySwitch가 인스턴스 참조 없이 트래킹을 요청하기 위한 정적 이벤트.
    // EnergyLineTracker가 씬에 없으면 null이므로 EnergySwitch는 안전하게 Invoke한다.
    public static Action<EnergyLine> OnTrackingRequested;

    private CameraController _cameraController;

    [SerializeField]
    private Image _fadePanel;

    [Foldout("Project")]
    [Tooltip("에너지 구가 라인 끝에 도달한 후 카메라가 머무는 시간 (초)")]
    [SerializeField]
    private float _holdDuration = 3f;

    [Tooltip("에너지 구를 향해 카메라가 이동하는 SmoothDamp 시간 (낮을수록 빠름)")]
    [SerializeField]
    private float _smoothTime = 0.15f;

    [Tooltip("페이드 아웃(검은 화면으로) 시간 (초)")]
    [SerializeField]
    private float _fadeOutDuration = 0.2f;

    [Tooltip("페이드 인(검은 화면 해제) 시간 (초)")]
    [SerializeField]
    private float _fadeInDuration = 0.2f;

    private EnergyLineTrackingCameraLogic _cameraLogic;
    private EnergyLineTrackingFadeLogic _fadeLogic;

    private bool _isRunning;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _cameraController = GetComponent<CameraController>();
        if (_cameraController == null)
        {
            Debug.LogError($"[EnergyLineTracker] {nameof(CameraController)} 컴포넌트를 동일 GameObject에서 찾을 수 없습니다.");
        }

        _cameraLogic = new EnergyLineTrackingCameraLogic();
        _fadeLogic = new EnergyLineTrackingFadeLogic(_fadePanel, _fadeOutDuration, _fadeInDuration);
    }

    private void OnEnable()
    {
        OnTrackingRequested += StartTracking;
    }

    private void LateUpdate()
    {
        _cameraLogic.UpdatePosition(transform, _smoothTime);
    }

    private void OnDisable()
    {
        OnTrackingRequested -= StartTracking;
        ForceCleanUp();
    }

    // EnergySwitch의 정적 이벤트 발행에 의해 호출된다.
    // 이미 트래킹이 진행 중이면 새 요청을 무시한다.
    private void StartTracking(EnergyLine targetLine)
    {
        if (_isRunning)
        {
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        PlayTrackingSequenceAsync(targetLine, _cts.Token).Forget();
    }

    private async UniTaskVoid PlayTrackingSequenceAsync(EnergyLine targetLine, CancellationToken ct)
    {
        _isRunning = true;
        InputHandler.BlockInput();

        try
        {
            _cameraLogic.StartTracking(targetLine, _cameraController);

            await WaitForSegmentToReachEndAsync(targetLine, ct);

            await UniTask.Delay(TimeSpan.FromSeconds(_holdDuration), cancellationToken: ct);

            await _fadeLogic.FadeOutAsync(ct);

            _cameraLogic.StopTracking(transform);

            await _fadeLogic.FadeInAsync(ct);
        }
        finally
        {
            CleanUp();
        }
    }

    // HeadDistance가 TotalDistance에 도달할 때까지 매 프레임 폴링한다.
    // 세그먼트가 예기치 않게 소멸된 경우에도 무한 대기를 방지한다.
    private async UniTask WaitForSegmentToReachEndAsync(EnergyLine targetLine, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (!targetLine.TryGetSegmentHeadDistance(out float headDistance))
            {
                break;
            }

            if (headDistance >= targetLine.TotalDistance)
            {
                break;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }

    private void CleanUp()
    {
        _isRunning = false;
        _fadeLogic.Reset();
        InputHandler.UnblockInput();

        _cts?.Dispose();
        _cts = null;
    }

    // OnDisable 등 외부 중단 시 트래킹 중이었다면 카메라와 입력을 강제 복구한다.
    private void ForceCleanUp()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _isRunning = false;

        if (_cameraLogic != null && _cameraLogic.IsTracking)
        {
            _cameraLogic.StopTracking(transform);
        }

        _fadeLogic?.Reset();
        InputHandler.UnblockInput();
    }
}
