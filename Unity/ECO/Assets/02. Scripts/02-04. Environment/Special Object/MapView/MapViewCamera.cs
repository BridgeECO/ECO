using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 맵 조망 중 메인 카메라의 위치와 시야 범위를 조작하는 순수 C# 클래스.
/// MonoBehaviour가 아니므로 컴포넌트로 부착하지 않으며, MapViewObject가 소유하고 구동한다.
///
/// 시야 범위는 FOV가 아니라 Z축으로 물러나서(dolly) 넓힌다. FOV를 바꾸면 원근 왜곡이 함께 변해
/// 배경과 전경의 상대 크기가 어긋나며, 프로젝트도 이미 보스방에서 Z를 -13으로 빼는 방식을 쓰고 있다.
/// </summary>
public class MapViewCamera
{
    // 카메라 Z를 기획자가 직접 입력하므로, 0에 가까운 값이 들어오면 콘텐츠 평면(z≈0)이
    // 근평면 안쪽에 들어와 화면이 비어 버린다. 그 경우만 막는 하한선.
    private const float MAX_CAMERA_Z = -1f;

    private CameraController _cameraController;
    private Transform _cameraTransform;
    private Tweener _moveTween;
    private float _originalZ;

    public bool IsViewing { get; private set; }

    // PersistentScene의 카메라가 Awake 시점에 아직 없을 수 있어 최초 상호작용 시점에 지연 바인딩한다.
    // CameraController는 Camera가 붙은 오브젝트가 아니라 그 부모에 있다.
    public bool TryBind()
    {
        if (_cameraController != null)
        {
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError($"[MapViewCamera] Main Camera를 찾을 수 없습니다. MainCamera 태그를 확인해 주세요.");
            return false;
        }

        _cameraController = mainCamera.GetComponentInParent<CameraController>();
        if (_cameraController == null)
        {
            Debug.LogError($"[MapViewCamera] CameraController를 찾을 수 없습니다.");
            return false;
        }

        _cameraTransform = _cameraController.transform;
        return true;
    }

    // 조망 지점에서는 XY만 쓰고, 얼마나 물러나 볼지는 cameraZ가 결정한다.
    public async UniTask EnterAsync(Vector3 viewWorldPosition, float cameraZ, float duration, CancellationToken cancellationToken)
    {
        // 조망 중 재진입 시 이미 물러난 Z를 원본으로 덮어쓰지 않도록 최초 1회만 기록한다.
        if (!IsViewing)
        {
            _originalZ = _cameraTransform.position.z;
        }

        IsViewing = true;
        _cameraController.IsFollowingPlayer = false;

        Vector3 targetPosition = new Vector3(viewWorldPosition.x, viewWorldPosition.y, ClampCameraZ(cameraZ));

        KillMoveTween();
        _moveTween = _cameraTransform.DOMove(targetPosition, duration).SetEase(Ease.InOutCubic);
        await _moveTween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
    }

    // 복귀 목표가 움직이는 플레이어라 고정 목표 트윈으로는 안착 지점이 어긋난다.
    // 매 프레임 목표를 다시 읽되 Lerp 계수는 경과 시간으로 몰아, 진행률 1에서 현재 목표에 정확히 닿게 한다.
    public async UniTask ExitAsync(float duration, CancellationToken cancellationToken)
    {
        KillMoveTween();

        Vector3 startPosition = _cameraTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float easedRatio = DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(elapsed / duration), Ease.InOutCubic);
            _cameraTransform.position = Vector3.Lerp(startPosition, GetReturnPosition(), easedRatio);
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);
        }

        _cameraTransform.position = GetReturnPosition();
        Release();
    }

    // 오브젝트 비활성화나 리스폰 리셋처럼 연출을 이어갈 수 없는 시점에 즉시 카메라를 되돌린다.
    public void StopImmediate()
    {
        if (!IsViewing)
        {
            return;
        }

        KillMoveTween();
        _cameraTransform.position = GetReturnPosition();
        Release();
    }

    // 룸 전환 시작 시 카메라 소유권을 CameraRoomTransition에게 넘긴다.
    // 그쪽이 새 방 기준 위치와 Z, IsFollowingPlayer를 모두 복구하므로
    // 여기서 되돌리면 이제 막 시작한 전환 연출을 덮어쓰게 된다.
    public void AbandonForRoomTransition()
    {
        if (!IsViewing)
        {
            return;
        }

        KillMoveTween();
        IsViewing = false;
    }

    private Vector3 GetReturnPosition()
    {
        Vector3 returnPosition = _cameraController.GetClampedPosition();
        returnPosition.z = _originalZ;
        return returnPosition;
    }

    private float ClampCameraZ(float cameraZ)
    {
        if (cameraZ <= MAX_CAMERA_Z)
        {
            return cameraZ;
        }

        Debug.LogWarning($"[MapViewCamera] Camera Z({cameraZ})가 너무 앞쪽이라 {MAX_CAMERA_Z}로 제한했습니다. -10 부근의 음수로 설정해 주세요.");
        return MAX_CAMERA_Z;
    }

    private void Release()
    {
        IsViewing = false;
        _cameraController.IsFollowingPlayer = true;
    }

    private void KillMoveTween()
    {
        _moveTween?.Kill();
        _moveTween = null;
    }
}
