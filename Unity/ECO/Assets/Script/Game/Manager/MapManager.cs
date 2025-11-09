using UnityEngine;

namespace ECO
{
    public class MapManager : IDestroyable
    {
        private Map _map = null;

        public bool Create(GameObject sceneRootGO)
        {
            if (!UNITY.TryFindCompWithName(out _map, "c_map", sceneRootGO))
                return false;

            return true;
        }

        public void Destroy()
        {
            UNITY.DestroyMono(ref _map);
        }
    }
}