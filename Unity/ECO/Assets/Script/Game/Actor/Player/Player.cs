using UnityEngine;

namespace ECO
{
    public class Player : MonoBase, IPlayer
    {
        private Rigidbody2D _rigid;

        public Transform TF => transform;
        public PlayerController Controller { get; set; }

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryGetComp(out _rigid, gameObject))
            {
                LOG.Error("Player: Rigidbody2D not found");
                return false;
            }

            _rigid.gravityScale = 2.5f;
            _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
            return true;
        }

        protected override void OnDestroyMono()
        {
            _rigid = null;
            Controller = null;
        }

        public void MoveHorizontal(float speedX)
        {
            if (_rigid == null) return;

            Vector2 v = _rigid.linearVelocity;
            v.x = speedX;
            _rigid.linearVelocity = v;
        }

        public void StopHorizontal()
        {
            if (_rigid == null) return;

            Vector2 v = _rigid.linearVelocity;
            v.x = 0f;
            _rigid.linearVelocity = v;
        }

        public void Jump(float power)
        {
            if (_rigid == null) return;

            Vector2 v = _rigid.linearVelocity;
            v.y = power;
            _rigid.linearVelocity = v;
        }

        protected override void OnCollisionEnterMono(Collision2D other)
        {
            if (other.contacts.Length == 0)
                return;

            if (other.contacts[0].normal.y > 0.5f)
                Controller?.OnGrounded();
        }

        protected override bool IsAutoShow()
        {
            return true;
        }
    }
}
