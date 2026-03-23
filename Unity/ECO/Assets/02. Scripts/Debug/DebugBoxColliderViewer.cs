using UnityEngine;

namespace ECODebug
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DebugBoxColliderViewer : MonoBehaviour
    {
        private BoxCollider2D _boxCol2D = null;
        private int _sn = 0;

        public void Awake()
        {
            _sn = DebugSystem.GenerateSN(this.gameObject);
            _boxCol2D = GetComponent<BoxCollider2D>();
        }

        public void Update()
        {
            DebugSystem.DrawBoxCol2D(_boxCol2D, _sn);
        }
    }
}