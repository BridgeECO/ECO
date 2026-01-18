using Org.BouncyCastle.Security;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ECO
{
    public class PlayerController : IPlayerController
    {
        private Player _player;
        private Rigidbody2D _rigid;

        [SerializeField]
        private float _moveSpeed = 10f;
        [SerializeField]
        private float _jumpPower = 20f;

        private bool _isGrounded;
        private bool _hasUsedAirJump;
        public bool isAirjumpable;

        private float _wallNormalX;

        public IPlayer Player => _player;

        //테스트용 임시 코드
        private TempTestResonanceController _resonanceController = null;

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

            isAirjumpable = false;

            _isGrounded = true;
            _hasUsedAirJump = false;
            _wallNormalX = 0f;

            //만약 해당 씬이 TempTest면 ResonanceController 받아오기
            if(SceneManager.GetActiveScene().name == "TempTest")
            {
                GameObject tempController = null;
                UNITY.TryFindGOWithName(out tempController, "TempTest_scene");
                _resonanceController = tempController.GetComponent<TempTestScene>()._resonanceCtrl;
            }

            return true;
        }

        public void ShowPlayer()
        {
            if (_player == null) return;
            _player.Show();
        }

        // �ܺο��� ������ �̵���Ű�� ���� ���� ��� (������ ���� �Է� ó�� ���)
        public void Move(Vector2 dir)
        {
            float speedX = dir.x * _moveSpeed;
            _player.MoveHorizontal(speedX);
        }

        // �ܺο��� ������ ȣ���ϰ� ���� �� ��� ����������,
        // �⺻�����δ� Update()���� �Է��� ���� �о� ó����
        public void Jump()
        {
            TryJump();
        }

        public void Update()
        {
            float moveX = HandleMoveInput();
            HandleJumpInput();
            ResolveWallStick(moveX);
            HandleInteractInput();
        }

        private void HandleInteractInput()
        {
            if(Input.GetKey(KeyCode.X))
            {
                Player nowPlayer;

                if(UNITY.TryFindCompWithName(out nowPlayer, "c_player"))
                {
                    if(nowPlayer.nowInteractObject != null)
                    {
                        nowPlayer.nowInteractObject.GetComponent<TempGainAirJumpObject>().GainAirJump(this);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private float HandleMoveInput()
        {
            if (_player == null)
                return 0f;

            float moveX = 0f;

            if (Input.GetKey(KeyCode.LeftArrow))
                moveX = -1f;
            else if (Input.GetKey(KeyCode.RightArrow))
                moveX = 1f;

            if (Mathf.Abs(moveX) < 0.01f)
                _player.StopHorizontal();
            else
                _player.MoveHorizontal(moveX * _moveSpeed);

            return moveX;
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
                _hasUsedAirJump = false;
                return;
            }

            if (!_hasUsedAirJump && isAirjumpable)
            {
                _player.Jump(_jumpPower);
                _hasUsedAirJump = true;

                //만약 TempTestResonanceController에 값이 있다면 사용하기
                if(_resonanceController != null)
                {
                    _resonanceController.OnPlayerAirJumped(_player.gameObject.transform);
                    Debug.Log("공명 발생");
                }
            }
        }

        private void ResolveWallStick(float moveX)
        {
            if (_player == null)
                return;

            if (_rigid == null)
                return;

            if (_isGrounded)
                return;

            if (Mathf.Abs(moveX) < 0.01f)
            {
                Debug.Log("movex");
                return;
            }

            if (Mathf.Abs(_wallNormalX) < 0.01f)
            {
                Debug.Log("wallnormalx");
                return;
            }

            float moveSign = moveX > 0f ? 1f : -1f;
            float wallSide = _wallNormalX > 0f ? 1f : -1f;

            if (moveSign != -wallSide)
                return;

            Vector2 lv = _rigid.linearVelocity;
            if (lv.x * moveSign > 0f)
                _rigid.linearVelocity = new Vector2(0f, lv.y);

            _player.StopHorizontal();
        }


        public void OnGrounded()
        {            
            _isGrounded = true;
            _hasUsedAirJump = false;
            _wallNormalX = 0f;
        }

        public void OnAirborne()
        {
            _isGrounded = false;
            _wallNormalX = 0f;
        }

        public void OnWallContact(float normalX)
        {
            _wallNormalX = normalX;
        }
    }
}
