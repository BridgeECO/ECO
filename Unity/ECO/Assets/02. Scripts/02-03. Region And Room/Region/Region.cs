using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Region : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private string _regionName;

    [SerializeField]
    private List<Room> _rooms;


    private Room _currentRoom;

    public string RegionName => _regionName;
    public IReadOnlyList<Room> Rooms => _rooms;

    private void Awake()
    {
        _currentRoom = _rooms[0];
    }
}