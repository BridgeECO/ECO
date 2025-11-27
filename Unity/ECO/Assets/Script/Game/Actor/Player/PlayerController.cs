using UnityEngine;

namespace ECO
{
    public class PlayerController : IPlayerController
    {
        private Player _player;
        private Rigidbody2D _rigid;

        private float _moveSpeed = 6f;
        private float _jumpPower = 8f;

        private int _curJumpCount;
        private const int MAX_JUMP_COUNT = 2;
        private bool _isGrounded;

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

            _curJumpCount = 0;
            _isGrounded = false;
            return true;
        }

        public void ShowPlayer()
        {
            if (_player == null) return;
            _player.Show();
        }

        // 외부에서 방향을 직접 밀어주고 싶을 때 사용 (현재 TestTutorial에서는 사용 안 함)
        public void Move(Vector2 dir)
        {
            float speedX = dir.x * _moveSpeed;
            _player.MoveHorizontal(speedX);
        }

        public void Jump()
        {
            if (_curJumpCount >= MAX_JUMP_COUNT)
                return;

            _player.Jump(_jumpPower);
            _curJumpCount++;
            _isGrounded = false;
        }

        public void Update()
        {
            HandleMoveInput();
        }

        private void HandleMoveInput()
        {
            if (_player == null)
                return;

            float moveX = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveX = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveX = 1f;

            if (Mathf.Abs(moveX) < 0.01f)
                _player.StopHorizontal();
            else
                _player.MoveHorizontal(moveX * _moveSpeed);
        }

        public void OnGrounded()
        {
            _isGrounded = true;
            _curJumpCount = 0;
        }
    }
}
