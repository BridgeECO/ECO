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

    private float _lastTriggerTime = -1f;
    private Collider2D _transitionCollider;

    private void Awake()
    {
        _transitionCollider = GetComponent<Collider2D>();
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

        Room actualRoom = GetRoomByAxis(other);
        if (actualRoom == null || actualRoom == Region.Instance.CurrentRoom)
        {
            return;
        }
        _lastTriggerTime = Time.time;
        ExecuteRoomTransition(actualRoom);
    }

    private Room GetTargetRoom()
    {
        Room currentRoom = Region.Instance.CurrentRoom;
        return (currentRoom == _roomB) ? _roomA : _roomB;
    }

    private void ExecuteRoomTransition(Room targetRoom)
    {
        if (targetRoom == null || Region.Instance.CurrentRoom == targetRoom)
        {
            return;
        }

        Vector3 spawnPosition = targetRoom == _roomA ? _spawnPointA.position : _spawnPointB.position;
        Region.Instance.SetCurrentRoom(targetRoom, spawnPosition);
    }

    private Room GetRoomByAxis(Collider2D playerCollider)
    {
        Vector2 playerPos = playerCollider.transform.position;
        Vector2 transitionCenter = _transitionCollider.bounds.center;

        return IsHorizontalCollider()
            ? GetRoomByHorizontalPosition(playerPos.x, transitionCenter.x, _roomA.Center.x, _roomB.Center.x)
            : GetRoomByVerticalPosition(playerPos.y, transitionCenter.y, _roomA.Center.y, _roomB.Center.y);
    }

    private bool IsHorizontalCollider()
    {
        Vector2 transitionSize = _transitionCollider.bounds.size;
        return transitionSize.y < transitionSize.x;
    }

    private Room GetRoomByHorizontalPosition(float playerX, float transitionCenterX, float centerAX, float centerBX)
    {
        bool isARightOfB = centerBX < centerAX;
        bool isPlayerRightOfTransition = playerX > transitionCenterX;
        return (isPlayerRightOfTransition == isARightOfB) ? _roomA : _roomB;
    }

    private Room GetRoomByVerticalPosition(float playerY, float transitionCenterY, float centerAY, float centerBY)
    {
        bool isAAboveB = centerBY < centerAY;
        bool isPlayerAboveTransition = playerY > transitionCenterY;
        return (isPlayerAboveTransition == isAAboveB) ? _roomA : _roomB;
    }
}