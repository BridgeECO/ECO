using UnityEngine;

namespace ECO
{
    public class Platform : MonoBase
    {
        [SerializeField] private AkEvent _wwiseEvt = null;


        protected override void OnDestroyMono()
        {

        }

        protected override bool OnCreateMono()
        {
            return true;
        }

        public void PlayWwiseEvt()
        {
            if (_wwiseEvt == null)
                return;

            AkSoundEngine.PostEvent(_wwiseEvt.playingId, this.gameObject);
        }
    }
}