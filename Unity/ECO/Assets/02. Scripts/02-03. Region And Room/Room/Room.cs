using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    private const int PPU = 256;
    private BoxCollider2D _cameraBounds;
    private List<IResettable> _resettables;

    public Vector2 MinBounds => _cameraBounds.bounds.min;
    public Vector2 MaxBounds => _cameraBounds.bounds.max;
    public bool IsVisited { get; set; }

    private void Awake()
    {
        _cameraBounds = GetComponent<BoxCollider2D>();
        _resettables = GetComponentsInChildren<IResettable>(true).ToList();
    }

    public void ResetRoom()
    {
        if (_resettables == null)
        {
            return;
        }

        for (int i = 0; i < _resettables.Count; i++)
        {
            _resettables[i]?.ResetState();
        }
    }

    [Button]
    public void SetThisRoomToCurrentRoom()
    {
        RespawnManager.Instance.UpdateSavePoint(this, transform.position);
        Region.Instance.SetCurrentRoom(this);
        EventManager.Instance.BroadcastEvent(EEventType.RoomChanged);
    }
}