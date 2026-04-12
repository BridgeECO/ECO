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

    private CameraRoomTransition _cameraRoomTransition;
    private float _lastTriggerTime = -1f;

    private void Start()
    {
        _cameraRoomTransition = FindAnyObjectByType<CameraRoomTransition>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - _lastTriggerTime < 0.5f)
        {
            return;
        }

        if (other.CompareTag(nameof(ETags.Player)))
        {
            _lastTriggerTime = Time.time;
            _cameraRoomTransition.StartRoomTransitionAsync
            (_targetRoom.MinBounds, _targetRoom.MaxBounds,
            this.GetCancellationTokenOnDestroy()).Forget();
            UpdatePlayerSavePoint(_savePoint.position);
        }
    }

    private void UpdatePlayerSavePoint(Vector3 newSavePoint)
    {
        RespawnManager.Instance.UpdateSavePoint(_targetRoom, newSavePoint);
    }
}