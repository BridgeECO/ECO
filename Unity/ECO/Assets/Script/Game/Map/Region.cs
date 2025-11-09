using UnityEngine;

namespace ECO
{
    public class Region : MonoBase
    {
        [SerializeField] private ERegion Type = ERegion.A;

        protected override bool OnCreateMono()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnDestroyMono()
        {
            throw new System.NotImplementedException();
        }

        public ERegion GetRegionType() => Type;
    }
}