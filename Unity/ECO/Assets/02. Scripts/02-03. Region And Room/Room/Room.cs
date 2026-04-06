using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    private const int PPU = 256;

    [Foldout("Hierarchy")]
    [SerializeField]
    private BoxCollider2D _cameraBounds;

    [SerializeField]
    private Transform _spawnPoint;

    [Foldout("Pixel Boundary")]
    [SerializeField]
    private float _pixelWidth;

    [SerializeField]
    private float _pixelHeight;

    public Vector2 MinBounds => _cameraBounds.bounds.min;
    public Vector2 MaxBounds => _cameraBounds.bounds.max;
    public Vector3 SpawnPoint => _spawnPoint.position;

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
}