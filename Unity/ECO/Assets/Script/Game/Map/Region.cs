using System.Collections.Generic;
using UnityEngine;

namespace ECO
{
    public class Region : MonoBase
    {
        [SerializeField] private ERegion Type = ERegion.A;
        private List<Room> _roomList = new();

        protected override bool OnCreateMono()
        {
            return true;
        }

        protected override void OnDestroyMono()
        {
            UNITY.DestroyMonoList(ref _roomList);
        }

        public ERegion GetRegionType() => Type;
    }
}