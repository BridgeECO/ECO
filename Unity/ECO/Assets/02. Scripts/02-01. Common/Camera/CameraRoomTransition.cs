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

    private CameraController _cameraController;
    private bool _isTransitioning;
    private CancellationTokenSource _transitionCts;
    private Tweener _transitionTween;

    public bool IsTransitioning => _isTransitioning;

    private void Awake()
    {
        _cameraController = GetComponent<CameraController>();
    }

    private void OnDestroy()
    {
        StopTransition();
    }


    public async UniTask StartRoomTransitionAsync(Vector2 nextRoomMin, Vector2 nextRoomMax, CancellationToken cancellationToken)
    {
        Debug.Log($"{nextRoomMin}, {nextRoomMax}");
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
        float distance = Vector3.Distance(transform.position, targetPosition);
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
        targetPosition.z = transform.position.z;
        return targetPosition;
    }
}