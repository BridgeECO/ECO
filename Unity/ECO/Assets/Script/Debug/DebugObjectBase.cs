using UnityEngine;

namespace ECODebug
{
    public abstract class DebugObjectBase
    {
        protected GameObject _go;
        protected Transform _tr;
        private int _sn = 0;

        public virtual void Create(GameObject root, int sn)
        {
            _go = new GameObject(GetType().Name);
            _tr = _go.transform;
            _tr.SetParent(root.transform, false);
            _go.SetActive(false);
            _sn = sn;
        }

        public bool IsSNEqual(int sn)
        {
            return _sn == sn;
        }

        public virtual void Show()
        {
            if (_go != null)
                _go.SetActive(true);
        }

        public virtual void Hide()
        {
            if (_go != null)
                _go.SetActive(false);
        }

        public virtual void Destroy()
        {
            if (_go != null)
                UnityEngine.Object.Destroy(_go);
        }
    }
}