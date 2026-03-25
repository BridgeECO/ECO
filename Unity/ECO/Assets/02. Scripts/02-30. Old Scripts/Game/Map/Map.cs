using System.Collections.Generic;
using UnityEngine;

namespace ECO
{
    public class Map : MonoBase
    {
        private List<Region> _regionList = new List<Region>();

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryFindGOWithName(out GameObject regionRootGO, "c_region_root", this.gameObject))
                return false;

            _regionList = UNITY.GetCompListInChild<Region>(regionRootGO);
            return true;
        }

        protected override void OnDestroyMono()
        {
            UNITY.DestroyMonoList(ref _regionList);
        }
    }
}