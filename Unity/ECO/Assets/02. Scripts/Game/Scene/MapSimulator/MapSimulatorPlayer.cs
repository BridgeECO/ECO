using UnityEngine;

namespace ECO
{
    public class MapSimulatorPlayer : MonoBase, IPlayer
    {
        public Transform TF => this.gameObject.transform;

        protected override bool OnCreateMono()
        {
            return true;
        }

        protected override void OnDestroyMono()
        {

        }
    }
}