using UnityEngine;

namespace ECO
{
    public class Player : MonoBase, IPlayer
    {
        private Rigidbody2D _rigid;

        public Transform TF => transform;
        public PlayerController Controller { get; set; }

        // 이번 물리 스텝(FixedUpdate) 동안 감지된 상태를 누적
        private bool _groundedThisStep;
        private bool _wallThisStep;
        private float _wallNormalX;

        public GameObject nowInteractObject;

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
            if(other.gameObject.tag == "InteractObject")
                return;
            
            ProcessContacts(other);
        }

        protected override void OnCollisionStayMono(Collision2D other)
        {
            if(other.gameObject.tag == "InteractObject")
                return;
            
            ProcessContacts(other);
        }

        private void FixedUpdate()
        {
            // 1) 이번 물리 스텝에서 충돌 콜백(Enter/Stay)이 오면 아래 값들이 채워짐
            // 2) 물리 스텝 끝에서 "딱 한번" 상태를 결정하고 Controller에 전달
            if (Controller == null)
            {
                ResetStepFlags();
                return;
            }

            if (_groundedThisStep)
            {
                Controller.OnGrounded();
            }
            else
            {
                // ⭐ 핵심: 땅이 아니더라도 "벽에 붙어 있으면" Airborne으로 리셋하지 않음
                // (너의 버그는 벽 접촉 중 grounded가 순간 끊기며 Airborne이 고정되는 케이스가 많음)
                if (_wallThisStep)
                    Controller.OnWallContact(_wallNormalX);
                else
                    Controller.OnAirborne();
            }

            ResetStepFlags();
        }

        private void ResetStepFlags()
        {
            _groundedThisStep = false;
            _wallThisStep = false;
            _wallNormalX = 0f;
        }

        private void ProcessContacts(Collision2D other)
        {
            if (Controller == null) return;
            if (other.contactCount == 0) return;

            for (int i = 0; i < other.contactCount; ++i)
            {
                // ✅ 이 한 줄의 의미:
                // other.contacts[i] = i번째 접촉점(ContactPoint2D)
                // .normal = 그 접촉점에서 "상대 표면이 나를 밀어내는 방향" (단위 벡터에 가까움)
                Vector2 n = other.contacts[i].normal;

                // 바닥 판정: 노멀의 y가 충분히 위를 향하면(경사 포함) grounded
                if (n.y > 0.5f)
                    _groundedThisStep = true;

                // 벽 판정: 노멀의 x가 거의 좌/우를 향하면 벽
                if (Mathf.Abs(n.x) > 0.9f)
                {
                    _wallThisStep = true;
                    _wallNormalX = n.x; // 마지막으로 감지된 벽 방향 저장
                }
            }
        }

        protected override bool IsAutoShow()
        {
            return true;
        }
    }
}
