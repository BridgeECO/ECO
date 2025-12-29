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
            ProcessContacts(other);
        }

        protected void OnCollisionStayMono(Collision2D other)
        {
            ProcessContacts(other);
        }

        protected override void OnCollisionExitMono(Collision2D other)
        {
            Controller?.OnAirborne();
        }

        private void ProcessContacts(Collision2D other)
        {
            if (Controller == null)
                return;

            if (other.contactCount == 0)
                return;

            bool grounded = false;
            bool wall = false;
            float wallNormalX = 0f;

            for (int i = 0; i < other.contactCount; ++i)
            {
                Vector2 n = other.contacts[i].normal;

                if (n.y > 0.5f)
                    grounded = true;

                if (Mathf.Abs(n.x) > 0.9f)
                {
                    wall = true;
                    wallNormalX = n.x;
                }
            }

            if (grounded)
            {
                Controller.OnGrounded();
                return;
            }

            Controller.OnAirborne();

            if (wall)
                Controller.OnWallContact(wallNormalX);
        }

        protected override bool IsAutoShow()
        {
            return true;
        }
    }
}
