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

    public string RegionName => _regionName;
    public IReadOnlyList<Room> Rooms => _rooms;
}