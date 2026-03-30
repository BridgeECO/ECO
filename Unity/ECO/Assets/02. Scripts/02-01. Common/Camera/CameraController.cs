using System;
using UnityEngine;
using VInspector;

public class CameraController : MonoBehaviour
{
    public Action OnRoomTransitionStarted;
    public Action OnRoomTransitionCompleted;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Transform _playerTransform;

    [Foldout("Project")]
    [Header("Room Bounds")]
    [SerializeField]
    private Vector2 _currentRoomMin;

    [SerializeField]
    private Vector2 _currentRoomMax;

    [Header("Camera Offset")]
    [SerializeField]
    private float _cameraYOffset;

    [Header("Room Transition")]
    [SerializeField]
    private float _roomTransitionSpeed;

    [SerializeField]
    private float _roomTransitionSnapThreshold;

    private bool _isTransitioning;
    private Vector3 _transitionTargetPosition;
    private float _halfCamHeight;
    private float _halfCamWidth;

    public bool IsTransitioning { get => _isTransitioning; private set => _isTransitioning = value; }

    private void Awake()
    {
        InitCameraHalfSize();
        Vector3 clamped = GetClampedPosition(_playerTransform.position);
        transform.position = clamped;
    }

    private void Start()
    {

    }

    private void LateUpdate()
    {
        if (_isTransitioning)
        {
            UpdateRoomTransition();
        }
        else
        {
            FollowPlayer();
        }
    }

    private void InitCameraHalfSize()
    {
        Camera camera = Camera.main;
        _halfCamHeight = camera.orthographicSize;
        _halfCamWidth = _halfCamHeight * camera.aspect;
    }

    private void UpdateRoomTransition()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _transitionTargetPosition,
            _roomTransitionSpeed * Time.deltaTime
        );

        float distanceToTarget = Vector3.Distance(transform.position, _transitionTargetPosition);
        if (distanceToTarget <= _roomTransitionSnapThreshold)
        {
            transform.position = _transitionTargetPosition;
            _isTransitioning = false;
            OnRoomTransitionCompleted?.Invoke();
        }
    }

    private void FollowPlayer()
    {
        Vector3 clamped = GetClampedPosition(_playerTransform.position);
        transform.position = clamped;
    }

    private Vector3 GetClampedPosition(Vector3 targetPosition)
    {
        float clampedX = ClampAxis(targetPosition.x, _currentRoomMin.x, _currentRoomMax.x, _halfCamWidth);
        float clampedY = ClampAxis(targetPosition.y - _cameraYOffset, _currentRoomMin.y, _currentRoomMax.y, _halfCamHeight);

        return new Vector3(clampedX, clampedY, transform.position.z);
    }

    private float ClampAxis(float target, float roomMin, float roomMax, float halfCamSize)
    {
        float clampMin = roomMin + halfCamSize;
        float clampMax = roomMax - halfCamSize;

        if (clampMin > clampMax)
        {
            return (roomMin + roomMax) * 0.5f;
        }

        return Mathf.Clamp(target, clampMin, clampMax);
    }

    public void StartRoomTransition(Vector2 nextRoomMin, Vector2 nextRoomMax)
    {
        _currentRoomMin = nextRoomMin;
        _currentRoomMax = nextRoomMax;

        Vector3 target = GetClampedPosition(_playerTransform.position);
        target.z = transform.position.z;
        _transitionTargetPosition = target;

        _isTransitioning = true;
        OnRoomTransitionStarted?.Invoke();
    }
}