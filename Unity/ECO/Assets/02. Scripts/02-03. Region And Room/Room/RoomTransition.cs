using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomTransition : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private Room _targetRoom;
    [SerializeField]
    private Transform _savePoint;

    [Foldout("Hierarchy")]
    [SerializeField]
    private CameraRoomTransition _cameraRoomTransition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Player)))
        {
            _cameraRoomTransition.StartRoomTransitionAsync
            (_targetRoom.MinBounds, _targetRoom.MaxBounds,
            this.GetCancellationTokenOnDestroy()).Forget();
            // UpdatePlayerSavePoint(_savePoint.position);
        }
    }

    private void UpdatePlayerSavePoint(Vector3 newSavePoint)
    {

    }
}