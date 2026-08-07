using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VInspector;

public class CameraController : MonoBehaviour
{
    // 플레이어와 같은 PersistentScene에 상주하므로 탐색 대신 인스펙터에서 직접 바인딩한다.
    [Foldout("Hierarchy")]
    [SerializeField]
    private Transform _followTarget;

    [Foldout("Settings")]
    [Header("Camera Offset")]
    [SerializeField]
    private float _cameraYOffset;

    [Header("Smooth Tracking")]
    [SerializeField]
    private float _trackingTimeAfterTransition = 0.3f;

    private Vector2 _currentRoomMin;
    private Vector2 _currentRoomMax;

    private float _halfCamHeight;
    private float _halfCamWidth;
    private Vector3 _velocity = Vector3.zero;

    private Camera _mainCamera;

    public bool IsFollowingPlayer { get; set; } = true;

    private void Awake()
    {
        if (_followTarget == null)
        {
            Debug.LogError($"[CameraController] 추적 대상(_followTarget)이 인스펙터에 할당되지 않았습니다.");
        }

        InitCameraController();

        if (_followTarget != null)
        {
            Vector3 clamped = GetClampedPosition();
            transform.position = clamped;
        }
    }

    private void LateUpdate()
    {
        if (!IsFollowingPlayer || _followTarget == null)
        {
            return;
        }
        FollowPlayer();
    }

    private void InitCameraController()
    {
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _halfCamHeight = _mainCamera.orthographicSize;
            _halfCamWidth = _halfCamHeight * _mainCamera.aspect;
        }
        else
        {
            Debug.LogError($"[CameraController] Main Camera를 찾을 수 없습니다. MainCamera 태그를 확인해 주세요.");
        }
    }

    private void FollowPlayer()
    {
        Vector3 targetPosition = GetClampedPosition();
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _trackingTimeAfterTransition);
    }

    private void UpdateCameraDimensions()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        _halfCamHeight = _mainCamera.orthographicSize;
        _halfCamWidth = _halfCamHeight * _mainCamera.aspect;
    }

    public void SetRoomBounds(Vector2 roomMin, Vector2 roomMax)
    {
        _currentRoomMin = roomMin;
        _currentRoomMax = roomMax;
    }

    public Vector3 GetClampedPosition()
    {
        float clampedX = ClampAxis(_followTarget.position.x, _currentRoomMin.x, _currentRoomMax.x, _halfCamWidth);
        float clampedY = ClampAxis(_followTarget.position.y + _cameraYOffset, _currentRoomMin.y, _currentRoomMax.y, _halfCamHeight);
        return new Vector3(clampedX, clampedY, transform.position.z);
    }
    public Vector3 GetClampedPosition(Vector3 targetPos)
    {
        UpdateCameraDimensions();

        float clampedX = ClampAxis(targetPos.x, _currentRoomMin.x, _currentRoomMax.x, _halfCamWidth);
        float clampedY = ClampAxis(targetPos.y + _cameraYOffset, _currentRoomMin.y, _currentRoomMax.y, _halfCamHeight);
        return new Vector3(clampedX, clampedY, transform.position.z);
    }

    private float ClampAxis(float target, float roomMin, float roomMax, float halfCamSize)
    {
        float clampMin = roomMin + halfCamSize;
        float clampMax = roomMax - halfCamSize;
        return (clampMax < clampMin) ? (roomMin + roomMax) * 0.5f : Mathf.Clamp(target, clampMin, clampMax);
    }

    public async UniTask PanToPositionAsync(Vector3 targetPosition, float duration, Ease ease = Ease.InOutCubic)
    {
        IsFollowingPlayer = false;

        await transform.DOMove(targetPosition, duration)
            .SetEase(ease)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public void MoveTowardsPosition(Vector3 targetPosition, float speed)
    {
        IsFollowingPlayer = false;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
}