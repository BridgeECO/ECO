using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 맵 조망 전환의 시작, 끝, 취소 토큰 수명을 관리하는 순수 C# 클래스.
/// MonoBehaviour가 아니므로 컴포넌트로 부착하지 않으며, MapViewObject가 소유하고 인스펙터 값을 인자로 넘겨 구동한다.
/// </summary>
public class MapViewSession
{
    private readonly MapViewCamera _mapViewCamera;
    private readonly CancellationToken _lifetimeToken;
    private CancellationTokenSource _viewCts;

    public bool IsViewing => _mapViewCamera.IsViewing;

    public MapViewSession(CancellationToken lifetimeToken)
    {
        _mapViewCamera = new MapViewCamera();
        _lifetimeToken = lifetimeToken;
    }

    public void StartView(Vector3 viewWorldPosition, float cameraZ, float enterDuration)
    {
        if (!_mapViewCamera.TryBind())
        {
            return;
        }

        RestartViewCts();
        EnterViewAsync(viewWorldPosition, cameraZ, enterDuration, _viewCts.Token).Forget();
    }

    public void StopView(float exitDuration)
    {
        // 진입이 방 전환 끝을 기다리는 동안에는 아직 조망 중이 아니어도 이탈이 온다.
        // 그때 토큰을 끊지 않으면 이미 나간 뒤에 줌아웃이 시작된다.
        if (!_mapViewCamera.IsViewing)
        {
            CancelViewCts();
            return;
        }

        RestartViewCts();
        ExitViewAsync(exitDuration, _viewCts.Token).Forget();
    }

    public void StopImmediate()
    {
        CancelViewCts();
        _mapViewCamera.StopImmediate();
    }

    public void AbandonForRoomTransition()
    {
        CancelViewCts();
        _mapViewCamera.AbandonForRoomTransition();
    }

    private async UniTaskVoid EnterViewAsync(Vector3 viewWorldPosition, float cameraZ, float enterDuration, CancellationToken cancellationToken)
    {
        try
        {
            await _mapViewCamera.EnterAsync(viewWorldPosition, cameraZ, enterDuration, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTaskVoid ExitViewAsync(float exitDuration, CancellationToken cancellationToken)
    {
        try
        {
            await _mapViewCamera.ExitAsync(exitDuration, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    // 조망 전환마다 끊을 수 있어야 하면서도, 소유 오브젝트가 파괴되면 함께 끊겨야 해서 수명 토큰에 연결한다.
    private void RestartViewCts()
    {
        CancelViewCts();
        _viewCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeToken);
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
}
