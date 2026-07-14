using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Offset")]
    [SerializeField]
    private float _cameraYOffset;

    [Header("Smooth Tracking")]
    [SerializeField]
    private float _trackingTimeAfterTransition = 0.3f;

    private Vector2 _currentRoomMin;
    private Vector2 _currentRoomMax;

    private Transform _playerTransform;

    private float _halfCamHeight;
    private float _halfCamWidth;
    private Vector3 _velocity = Vector3.zero;

    private Camera _mainCamera;

    public bool IsFollowingPlayer { get; set; } = true;

    private void Awake()
    {
        PlayerStateMachine playerStateMachine = FindFirstObjectByType<PlayerStateMachine>(FindObjectsInactive.Include);
        if (playerStateMachine != null)
        {
            _playerTransform = playerStateMachine.transform;
        }
        else
        {
            Debug.LogError($"[CameraController] PlayerStateMachine을 씬에서 찾을 수 없습니다.");
        }

        InitCameraController();

        if (_playerTransform != null)
        {
            Vector3 clamped = GetClampedPosition();
            transform.position = clamped;
        }
    }

    private void LateUpdate()
    {
        if (!IsFollowingPlayer)
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
        float clampedX = ClampAxis(_playerTransform.position.x, _currentRoomMin.x, _currentRoomMax.x, _halfCamWidth);
        float clampedY = ClampAxis(_playerTransform.position.y + _cameraYOffset, _currentRoomMin.y, _currentRoomMax.y, _halfCamHeight);
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

    public async UniTask PlayPanToTargetAsync(Transform target, float moveDuration, float holdDuration, float returnDuration)
    {
        IsFollowingPlayer = false;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y + _cameraYOffset, transform.position.z);

        await transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutCubic)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        await UniTask.Delay(System.TimeSpan.FromSeconds(holdDuration));

        Vector3 returnPos = GetClampedPosition();
        await transform.DOMove(returnPos, returnDuration)
            .SetEase(Ease.InOutCubic)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        IsFollowingPlayer = true;
    }
}