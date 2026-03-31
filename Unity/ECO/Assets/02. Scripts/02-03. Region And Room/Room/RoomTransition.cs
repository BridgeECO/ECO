using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class RoomTransition : MonoBehaviour
{
    [SerializeField]
    private Room _targetRoom;
    [SerializeField]
    private Transform _savePoint;
    [SerializeField]
    private CameraRoomTransition _cameraRoomTransition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Player)))
        {
            _cameraRoomTransition.StartRoomTransitionAsync
            (_targetRoom.MinBounds, _targetRoom.MaxBounds, CancellationToken.None).Forget();

            UpdatePlayerSavePoint(_savePoint.position);
        }
    }

    private void UpdatePlayerSavePoint(Vector3 newSavePoint)
    {

    }
}