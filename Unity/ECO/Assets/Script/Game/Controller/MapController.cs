using System.Collections.Generic;
using UnityEngine;

namespace ECO
{
    public class MapController : IDestroyable
    {
        private Map _map = null;
        private List<Region> _regionList = new();

        public bool Create(GameObject sceneRootGO)
        {
            if (!UNITY.TryFindCompWithName(out _map, "c_map", sceneRootGO))
                return false;

            _regionList = UNITY.GetCompListInChild<Region>(_map.gameObject);

            return true;
        }

        public void Destroy()
        {
            UNITY.DestroyMonoList(ref _regionList);
            UNITY.DestroyMono(ref _map);
        }
    }
}