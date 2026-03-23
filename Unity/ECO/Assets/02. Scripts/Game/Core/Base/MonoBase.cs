using UnityEngine;


namespace ECO
{
    public abstract class MonoBase : MonoBehaviour
    {
        private enum EShowState
        {
            HIDE,
            SHOW,
        }

        private bool _isCreateComplete = false;

        public bool IsCreateInRuntime { get; private set; }
        private EShowState _curShowState = EShowState.HIDE;

        private void OnDestroy()
        {
            Destroy();
        }

        private void Awake()
        {
            Create();
        }

        public bool Create()
        {
            if (_isCreateComplete)
                return true;

            if (!OnCreateMono())
            {
                //LOG.E($"Create Mono Failed. GameObject({this})");
                Hide();
                return false;
            }

            _isCreateComplete = true;

            if (IsAutoShow())
                Show();
            else
                Hide();

            return true;
        }

        private void Update()
        {
            OnUpdateMono();
        }

        private void FixedUpdate()
        {
            OnFixedUpdateMono();
        }

        private void LateUpdate()
        {
            OnLateUpdateMono();
        }

        private void OnDisable()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnTriggerEnterMono(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            OnTriggerExitMono(other);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            OnCollisionEnterMono(other);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            OnCollisionExitMono(other);
        }

        private void OnRectTransformDimensionsChange()
        {
            OnResizeScreen();
        }

        public void Destroy()
        {
            OnDestroyMono();

            _isCreateComplete = false;
        }

        protected virtual bool IsAutoShow() { return false; }

        protected abstract void OnDestroyMono();
        protected abstract bool OnCreateMono();
        protected virtual void OnShowMono() { }
        protected virtual void OnHideMono() { }
        protected virtual void OnUpdateMono() { }
        protected virtual void OnFixedUpdateMono() { }
        protected virtual void OnLateUpdateMono() { }
        protected virtual void OnResizeScreen() { }
        protected virtual void OnTriggerEnterMono(Collider2D other) { }
        protected virtual void OnTriggerExitMono(Collider2D other) { }
        protected virtual void OnCollisionEnterMono(Collision2D other) { }
        protected virtual void OnCollisionExitMono(Collision2D other) { }

        public void Show()
        {
            if (!_isCreateComplete)
            {
                if (!Create())
                    return;
            }

            if (IsShow())
                return;

            this.gameObject.SetActive(true);

            OnShowMono();

            _curShowState = EShowState.SHOW;
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
            _curShowState = EShowState.HIDE;
        }

        public bool IsShow()
        {
            return _curShowState == EShowState.SHOW;
        }

        public void SetIsCreateInRuntime(bool isCreateInRuntime)
        {
            this.IsCreateInRuntime = isCreateInRuntime;
        }


        public override string ToString()
        {
            //if (UNITY_UTIL.IsNullGameObj(this.gameObject))
            //{
            //    LOG.E("Invalid GameObject");
            //    return "Invalid GameObject";
            //}

            return this.gameObject.name;
        }
    }
}