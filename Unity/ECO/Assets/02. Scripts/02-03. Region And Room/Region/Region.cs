using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Region : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private List<Room> _rooms;

    private Room _currentRoom;

    public IReadOnlyList<Room> Rooms => _rooms;

    private void Awake()
    {
        _currentRoom = _rooms[0];
    }
}