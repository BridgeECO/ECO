using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VInspector;

public class CameraRoomTransition : MonoBehaviour
{
    public Action OnRoomTransitionStarted;
    public Action OnRoomTransitionCompleted;

    [Foldout("Project")]
    [Header("Room Transition")]
    [SerializeField]
    private float _roomTransitionDuration;

    [Header("BossRoom")]
    [SerializeField, Tooltip("보스방일 때 카메라의 Z값")]
    private float _bossRoomZValue;
    [SerializeField, Tooltip("일반방일 때 카메라의 Z값")]
    private float _defaultZValue;

    private CameraController _cameraController;
    private bool _isTransitioning;
    private CancellationTokenSource _transitionCts;
    private Tweener _transitionTween;
    private float _targetZ;

    public bool IsTransitioning => _isTransitioning;

    private void Awake()
    {
        _cameraController = GetComponent<CameraController>();
        _targetZ = _defaultZValue;
    }

    private void OnEnable()
    {
        if (EventManager.Instance == null)
        {
            return;
        }
        EventManager.Instance.AddEventListener(EEventType.RoomChanged, OnRoomChanged);
    }

    private void OnDisable()
    {
        RemoveEventListeners();
    }

    private void OnDestroy()
    {
        StopTransition();
    }

    private void RemoveEventListeners()
    {
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.RoomChanged, OnRoomChanged);
        }
    }

    private void OnRoomChanged()
    {
        Room currentRoom = Region.Instance.CurrentRoom;
        if (currentRoom == null)
        {
            return;
        }

        if (currentRoom.RoomType == ERoomType.Boss)
        {
            _targetZ = _bossRoomZValue;
        }
        else
        {
            _targetZ = _defaultZValue;
        }
        StartRoomTransitionAsync(currentRoom.MinBounds, currentRoom.MaxBounds, this.GetCancellationTokenOnDestroy()).Forget();
    }


    public async UniTask StartRoomTransitionAsync(Vector2 nextRoomMin, Vector2 nextRoomMax, CancellationToken cancellationToken)
    {
        if (_isTransitioning)
        {
            StopTransition();
        }
        _transitionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            InitTransition(nextRoomMin, nextRoomMax);
            await UpdateTransitionAsync(_transitionCts.Token);
            CompleteTransition();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void InitTransition(Vector2 nextRoomMin, Vector2 nextRoomMax)
    {
        _isTransitioning = true;
        _cameraController.IsFollowingPlayer = false;
        _cameraController.SetRoomBounds(nextRoomMin, nextRoomMax);
        OnRoomTransitionStarted?.Invoke();
    }

    private async UniTask UpdateTransitionAsync(CancellationToken cancellationToken)
    {
        Vector3 targetPosition = GetTargetPosition();
        _transitionTween = transform.DOMove(targetPosition, _roomTransitionDuration).SetEase(Ease.InOutSine);
        await _transitionTween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
    }

    private void CompleteTransition()
    {
        _isTransitioning = false;
        _cameraController.IsFollowingPlayer = true;
        OnRoomTransitionCompleted?.Invoke();
    }

    private void StopTransition()
    {
        _transitionCts?.Cancel();
        _transitionCts?.Dispose();
        _transitionTween?.Kill();
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition = _cameraController.GetClampedPosition();
        targetPosition.z = _targetZ;
        return targetPosition;
    }
}