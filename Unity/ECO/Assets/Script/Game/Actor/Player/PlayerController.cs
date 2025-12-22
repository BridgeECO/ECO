using UnityEngine;

namespace ECO
{
    public class PlayerController : IPlayerController
    {
        private Player _player;
        private Rigidbody2D _rigid;

        [SerializeField]
        private float _moveSpeed = 6f;
        [SerializeField]
        private float _jumpPower = 8f;

        private bool _isGrounded;
        private bool _canDoubleJump;

        public IPlayer Player => _player;

        public bool Create(GameObject sceneRootGO)
        {
            if (!UNITY.TryFindCompWithName(out _player, "c_player", sceneRootGO))
            {
                LOG.Error("PlayerController: c_player not found");
                return false;
            }

            if (!UNITY.TryGetComp(out _rigid, _player.gameObject))
            {
                LOG.Error("PlayerController: Rigidbody2D not found on Player");
                return false;
            }

            _rigid.gravityScale = 2.5f;
            _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            _player.Controller = this;

            _isGrounded = true;
            _canDoubleJump = true;
            return true;
        }

        public void ShowPlayer()
        {
            if (_player == null) return;
            _player.Show();
        }

        // 외부에서 강제로 이동시키고 싶을 때만 사용 (지금은 내부 입력 처리 사용)
        public void Move(Vector2 dir)
        {
            float speedX = dir.x * _moveSpeed;
            _player.MoveHorizontal(speedX);
        }

        // 외부에서 점프를 호출하고 싶을 때 사용 가능하지만,
        // 기본적으로는 Update()에서 입력을 직접 읽어 처리함
        public void Jump()
        {
            TryJump();
        }

        public void Update()
        {
            HandleMoveInput();
            HandleJumpInput();
        }

        private void HandleMoveInput()
        {
            if (_player == null)
                return;

            float moveX = 0f;

            if (Input.GetKey(KeyCode.LeftArrow))
                moveX = -1f;
            else if (Input.GetKey(KeyCode.RightArrow))
                moveX = 1f;

            if (Mathf.Abs(moveX) < 0.01f)
                _player.StopHorizontal();
            else
                _player.MoveHorizontal(moveX * _moveSpeed);
        }

        private void HandleJumpInput()
        {
            if (Input.GetKeyDown(KeyCode.Z))
                TryJump();
        }

        private void TryJump()
        {
            if (_player == null)
                return;

            if (_isGrounded)
            {
                _player.Jump(_jumpPower);
                _isGrounded = false;
                _canDoubleJump = true;
                return;
            }

            if (_canDoubleJump)
            {
                _player.Jump(_jumpPower);
                _canDoubleJump = false;
            }
        }

        public void OnGrounded()
        {
            _isGrounded = true;
            _canDoubleJump = true;
        }
    }
}
