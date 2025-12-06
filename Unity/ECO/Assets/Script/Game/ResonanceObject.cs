using UnityEngine;

namespace ECO
{
    public class ResonanceObject : MonoBase
    {
        private AnimationAssist _animAssist = null;

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryGetComp(out _animAssist, this.gameObject)) return false;

            return true;
        }

        protected override void OnDestroyMono()
        {
            _animAssist?.Destroy();
        }

        public void SetPos(Vector3 pos)
        {
            this.transform.position = pos;
        }

        public void PlayAnim(string animName)
        {
            _animAssist.Play(animName);
        }
    }
}