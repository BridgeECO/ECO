using System.Collections.Generic;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Transform _spawnPointForDebug;

    private BoxCollider2D _cameraBounds;
    private List<IResettable> _resettables;

    public Vector2 MinBounds => _cameraBounds.bounds.min;
    public Vector2 MaxBounds => _cameraBounds.bounds.max;
    public bool IsVisited { get; set; }

    private void Awake()
    {
        _cameraBounds = GetComponent<BoxCollider2D>();
        _resettables = new List<IResettable>();
        GetComponentsInChildren<IResettable>(true, _resettables);
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
    private void SetThisRoomToCurrentRoom()
    {
        Vector3 spawnPoint = _spawnPointForDebug != null ? _spawnPointForDebug.position : transform.position;
        DebugTool.ChangeCurrentRoom(this, spawnPoint);
    }
}