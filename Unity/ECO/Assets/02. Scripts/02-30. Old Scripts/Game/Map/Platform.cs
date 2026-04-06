using UnityEngine;

namespace ECO
{
    public class Platform : MonoBase
    {
        // [SerializeField] 
        // private AkEvent _wwiseEvt = null;

        private ResonanceObject _resonanceObj = null;


        protected override void OnDestroyMono()
        {
            _resonanceObj.onResonanceActivate.RemoveListener(PlayWwiseEvt);
        }

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryGetComp(out _resonanceObj, this.gameObject))
                return false;

            _resonanceObj.onResonanceActivate.AddListener(PlayWwiseEvt);
            return true;
        }

        protected override bool IsAutoShow()
        {
            return true;
        }

        public void PlayWwiseEvt()
        {
            LOG.Info("여기서 플랫폼에 할당된 음악들 플레이");
            // if (_wwiseEvt == null)
            //     return;
            // AkSoundEngine.PostEvent(_wwiseEvt.playingId, this.gameObject);
        }
    }
}