using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private BoxCollider2D _cameraBounds;

    [SerializeField]
    private Transform _spawnPoint;

    public Vector2 MinBounds => _cameraBounds.bounds.min;
    public Vector2 MaxBounds => _cameraBounds.bounds.max;
    public Vector3 SpawnPoint => _spawnPoint.position;
}