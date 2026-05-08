using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    private const int PPU = 256;

    [Foldout("Hierarchy")]
    [SerializeField]
    private BoxCollider2D _cameraBounds;

    [Foldout("Pixel Boundary")]
    [SerializeField]
    private float _pixelWidth;

    [SerializeField]
    private float _pixelHeight;

    private List<IRoomResettable> _resettables;

    public Vector2 MinBounds => _cameraBounds.bounds.min;
    public Vector2 MaxBounds => _cameraBounds.bounds.max;
    public bool IsVisited { get; set; }

    private void Awake()
    {
        _resettables = GetComponentsInChildren<IRoomResettable>(true).ToList();
    }

    [Button]
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
    private void RefreshBoundary()
    {
        if (_cameraBounds != null)
        {
            _cameraBounds.isTrigger = true;
            _cameraBounds.size = new Vector2(_pixelWidth / PPU, _pixelHeight / PPU);
            _cameraBounds.compositeOperation = Collider2D.CompositeOperation.Merge;
        }
    }

    [Button]
    public void SetThisRoomToCurrentRoom()
    {
        RespawnManager.Instance.UpdateSavePoint(this, transform.position);
    }
}