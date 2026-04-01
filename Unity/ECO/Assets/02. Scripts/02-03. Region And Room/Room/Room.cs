using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    [SerializeField]
    private Region _parentRegion;
    [SerializeField]
    private BoxCollider2D _cameraBounds;

    public Vector2 MinBounds => _cameraBounds.bounds.min;
    public Vector2 MaxBounds => _cameraBounds.bounds.max;
    public Region ParentRegion => _parentRegion;
}