using UnityEngine;

namespace ECODebug
{
    public class DebugBoxObject : DebugObjectBase
    {
        private LineRenderer _lr;

        public DebugBoxObject()
        {

        }

        public override void Create(GameObject root, int sn)
        {
            base.Create(root, sn);

            _lr = _go.AddComponent<LineRenderer>();
            _lr.positionCount = 5;
            _lr.loop = true;
            _lr.useWorldSpace = true;
            _lr.widthMultiplier = 0.02f;
        }

        public void Set(Collider2D col)
        {
            var bounds = col.bounds;

            _lr.SetPosition(0, new Vector3(bounds.min.x, bounds.min.y));
            _lr.SetPosition(1, new Vector3(bounds.max.x, bounds.min.y));
            _lr.SetPosition(2, new Vector3(bounds.max.x, bounds.max.y));
            _lr.SetPosition(3, new Vector3(bounds.min.x, bounds.max.y));
            _lr.SetPosition(4, new Vector3(bounds.min.x, bounds.min.y));
        }
    }
}