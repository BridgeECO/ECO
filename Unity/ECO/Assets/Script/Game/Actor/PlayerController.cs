using UnityEngine;

namespace ECO
{
    public class PlayerController : MonoBase
    {
        private Rigidbody2D _rigid;
        private float _moveSpeed = 5f;
        private float _jumpForce = 7f;
        private bool _isGrounded = false;

        protected override bool OnCreateMono()
        {
            // Rigidbody2D 가져오기
            if (!UNITY.TryGetComp(out _rigid, this.gameObject))
            {
                LOG.Error("PlayerController: Rigidbody2D not found");
                return false;
            }

            // Rigidbody 초기 세팅
            _rigid.gravityScale = 2.5f;
            _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
            return true;
        }

        protected override void OnDestroyMono()
        {
            _rigid = null;
        }

        protected override void OnUpdateMono()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            // 좌우 이동 입력
            float moveX = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveX = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveX = 1f;

            // 이동 처리 (linearVelocity 사용)
            Vector2 velocity = _rigid.linearVelocity;
            velocity.x = moveX * _moveSpeed;
            _rigid.linearVelocity = velocity;

            // 점프 (Space / W / ↑)
            if (_isGrounded && (Input.GetKeyDown(KeyCode.Space) ||
                                Input.GetKeyDown(KeyCode.W) ||
                                Input.GetKeyDown(KeyCode.UpArrow)))
            {
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, _jumpForce);
                _isGrounded = false;
            }
        }

        // 착지 감지용 트리거 처리
        protected override void OnCollisionEnterMono(Collision2D other)
        {
            if (other.contacts.Length == 0)
                return;

            // 아래쪽 충돌만 체크
            if (other.contacts[0].normal.y > 0.5f)
                _isGrounded = true;
        }

        protected override void OnCollisionExitMono(Collision2D other)
        {
            _isGrounded = false;
        }

        protected override bool IsAutoShow() { return true; }
    }
}
