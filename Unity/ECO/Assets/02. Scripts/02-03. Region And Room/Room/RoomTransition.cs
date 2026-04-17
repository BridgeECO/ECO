using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Collider2D))]
public class RoomTransition : MonoBehaviour
{
    [Foldout("Hierarchy")]
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
        if (!other.CompareTag(nameof(ETags.PlayerInteract)) || Time.time - _lastTriggerTime < 0.5f)
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

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            return;
        }

        Room currentRoom = RespawnManager.Instance.CurrentRoom;
        Room actualRoom = GetRoomByPosition(other);
        if (actualRoom != null && actualRoom != currentRoom)
        {
            _lastTriggerTime = Time.time;
            ExecuteRoomTransition(actualRoom);
        }
    }

    private Room GetTargetRoom()
    {
        Room currentRoom = RespawnManager.Instance.CurrentRoom;
        return (currentRoom == _roomB) ? _roomA : _roomB;
    }

    private void ExecuteRoomTransition(Room targetRoom)
    {
        if (targetRoom == null || RespawnManager.Instance.CurrentRoom == targetRoom)
        {
            return;
        }

        _cameraRoomTransition.StartRoomTransitionAsync
            (targetRoom.MinBounds, targetRoom.MaxBounds,
            this.GetCancellationTokenOnDestroy()).Forget();

        Vector3 spawnPosition = targetRoom == _roomA ? _spawnPointA.position : _spawnPointB.position;
        RespawnManager.Instance.UpdateSavePoint(targetRoom, spawnPosition);
        SaveManager.Instance.Save(targetRoom);
    }

    private Room GetRoomByPosition(Collider2D playerCollider)
    {
        Vector2 playerPos = playerCollider.transform.position;
        float distA = Vector2.SqrMagnitude(playerPos - (Vector2)_spawnPointA.position);
        float distB = Vector2.SqrMagnitude(playerPos - (Vector2)_spawnPointB.position);

        if (Mathf.Abs(distA - distB) < 0.1f)
        {
            Room velocityRoom = GetRoomByVelocity(playerCollider, (Vector3)playerPos);
            if (velocityRoom != null)
            {
                return velocityRoom;
            }
        }

        if (distA < distB)
        {
            return _roomA;
        }

        if (distB < distA)
        {
            return _roomB;
        }
        return RespawnManager.Instance.CurrentRoom;
    }

    private Room GetRoomByVelocity(Collider2D playerCollider, Vector3 position)
    {
        if (!playerCollider.TryGetComponent<Rigidbody2D>(out var rb))
        {
            return null;
        }

        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f)
        {
            return null;
        }

        Vector2 centerA = (_roomA.MinBounds + _roomA.MaxBounds) / 2f;
        Vector2 centerB = (_roomB.MinBounds + _roomB.MaxBounds) / 2f;
        Vector2 dirToA = (centerA - (Vector2)position).normalized;
        Vector2 dirToB = (centerB - (Vector2)position).normalized;

        float dotA = Vector2.Dot(velocity.normalized, dirToA);
        float dotB = Vector2.Dot(velocity.normalized, dirToB);

        if (dotA > dotB)
        {
            return _roomA;
        }
        if (dotB > dotA)
        {
            return _roomB;
        }
        return null;
    }
}