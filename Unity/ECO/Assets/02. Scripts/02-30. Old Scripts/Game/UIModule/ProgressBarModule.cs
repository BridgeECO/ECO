using UnityEngine.UI;

namespace ECO
{
    public class ProgressBarModule : MonoBase
    {
        private Image _progressImg = null;
        private Image _fullImg = null;

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryFindCompWithName(out _progressImg, "c_progress_img", this.gameObject))
                return false;

            if (!UNITY.TryFindCompWithName(out _fullImg, "c_full_img", this.gameObject))
                return false;

            if (_progressImg.type != Image.Type.Filled)
            {
                LOG.Error("ProgressImg Must Be Filled");
                return false;
            }

            return true;
        }

        protected override void OnDestroyMono()
        {
            _progressImg = null;
            _fullImg = null;
        }

        public void SetValue(float curValue, float maxValue)
        {
            float value = curValue / maxValue;
            if (value >= 1f)
            {
                value = 1f;
                _fullImg.gameObject.SetActive(true);
            }
            else
            {
                _fullImg.gameObject.SetActive(false);
            }

            _progressImg.fillAmount = value;
        }
    }
}