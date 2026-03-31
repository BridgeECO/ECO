using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    [SerializeField]
    private string _regionName;

    [SerializeField]
    private List<Room> _rooms = new List<Room>();

    public string RegionName => _regionName;
    public IReadOnlyList<Room> Rooms => _rooms;
}