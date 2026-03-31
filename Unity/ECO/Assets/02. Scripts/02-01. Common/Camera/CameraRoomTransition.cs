using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

public class CameraRoomTransition : MonoBehaviour
{
    public Action OnRoomTransitionStarted;
    public Action OnRoomTransitionCompleted;

    [Foldout("Hierarchy")]
    [SerializeField]
    private CameraController _cameraController;

    [SerializeField]
    private Transform _playerTransform;

    [Foldout("Project")]
    [Header("Room Transition")]
    [SerializeField]
    private float _roomTransitionSpeed;

    [SerializeField]
    private float _roomTransitionSnapThreshold;

    private bool _isTransitioning;

    public bool IsTransitioning => _isTransitioning;

    public async UniTask StartRoomTransitionAsync(Vector2 nextRoomMin, Vector2 nextRoomMax, CancellationToken cancellationToken)
    {
        if (_isTransitioning)
        {
            return;
        }
        InitTransition(nextRoomMin, nextRoomMax);
        await UpdateTransitionAsync(cancellationToken);
        CompleteTransition();
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
        while (true)
        {
            Vector3 targetPosition = GetTargetPosition();
            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

            if (distanceToTarget <= _roomTransitionSnapThreshold)
            {
                transform.position = targetPosition;
                break;
            }

            MoveTowardsTarget(targetPosition);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }

    private void CompleteTransition()
    {
        _isTransitioning = false;
        _cameraController.IsFollowingPlayer = true;
        OnRoomTransitionCompleted?.Invoke();
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition = _cameraController.GetClampedPosition(_playerTransform.position);
        targetPosition.z = transform.position.z;
        return targetPosition;
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            _roomTransitionSpeed * Time.deltaTime
        );
    }
}
