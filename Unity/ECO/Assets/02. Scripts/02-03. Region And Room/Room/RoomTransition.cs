using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomTransition : MonoBehaviour
{
    [Foldout("Rooms")]
    [SerializeField]
    private Room _roomA;
    [SerializeField]
    private Transform _spawnPointA;

    [SerializeField]
    private Room _roomB;
    [SerializeField]
    private Transform _spawnPointB;

    private CameraRoomTransition _cameraRoomTransition;
    private float _lastTriggerTime = -1f;

    private void Start()
    {
        _cameraRoomTransition = Camera.main.GetComponent<CameraRoomTransition>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - _lastTriggerTime < 0.5f)
        {
            return;
        }

        if (!other.CompareTag(nameof(ETags.Player)))
        {
            return;
        }


        Room targetRoom = GetTargetRoom();
        if (targetRoom == null)
        {
            return;
        }
        _lastTriggerTime = Time.time;
        ExecuteRoomTransition(targetRoom);
    }

    private void ExecuteRoomTransition(Room targetRoom)
    {
        _cameraRoomTransition.StartRoomTransitionAsync
            (targetRoom.MinBounds, targetRoom.MaxBounds,
            this.GetCancellationTokenOnDestroy()).Forget();

        Vector3 spawnPosition = targetRoom == _roomA ? _spawnPointA.position : _spawnPointB.position;
        RespawnManager.Instance.UpdateSavePoint(targetRoom, spawnPosition);

        // if (targetRoom.IsVisited)
        // {
        //     return;
        // }
        // targetRoom.IsVisited = true;
        SaveManager.Instance.Save(targetRoom);
    }

    private Room GetTargetRoom()
    {
        Room currentRoom = RespawnManager.Instance.CurrentRoom;
        if (currentRoom == _roomA)
        {
            return _roomB;
        }
        else if (currentRoom == _roomB)
        {
            return _roomA;
        }
        else if (currentRoom == null)
        {
            return _roomB;
        }
        return null;
    }
}