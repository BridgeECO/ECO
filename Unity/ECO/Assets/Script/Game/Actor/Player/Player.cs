using UnityEngine;
using UnityEngine.UIElements;

namespace ECO
{
    public class Player : MonoBase, IPlayer
    {
        private Rigidbody2D _rigid;
        private BoxCollider2D _box2D;

        public Transform TF => transform;
        public PlayerController Controller { get; set; }

        // 이번 물리 스텝(FixedUpdate) 동안 감지된 상태를 누적
        private bool _groundedThisStep;
        private bool _wallThisStep;
        private float _wallNormalX;

        [Header("Jump Settings")]
        public float jumpVelocity = 20f;
        public float maxHoldTime = 0.5f;
        private float jumpStartTime;
        
        [Header("Gravity Scaling")]       // 점프 초기 속도
        public float defaultGravity;
        public float maxGravityOnRelease; // 아주 짧게 눌렀을 때 적용될 강한 중력
        public float minGravityOnRelease; // 최대 시간 근처로 눌렀을 때 적용될 약한 중력

        public GameObject nowInteractObject;

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryGetComp(out _rigid, gameObject))
            {
                LOG.Error("Player: Rigidbody2D not found");
                return false;
            }
            _box2D = GetComponent<BoxCollider2D>();

            defaultGravity = 3.5f;
            maxGravityOnRelease = 9f;
            minGravityOnRelease = 4f;

            _rigid.gravityScale = defaultGravity;
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

            jumpStartTime = Time.time;
        }

        protected override void OnCollisionEnterMono(Collision2D other)
        {            
            ProcessContacts(other);
        }

        protected override void OnCollisionStayMono(Collision2D other)
        {
            ProcessContacts(other);
        }

        private void Update()
        {
            FallingDown();
        }

        private void FixedUpdate()
        {
            CheckPlayerState();
        }

        private void FallingDown()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Debug.Log("xxxx");
            }

            // 1. 점프 중 키에서 손을 뗐을 때
            if (Input.GetKeyUp(KeyCode.Z) && _rigid.linearVelocityY > 0)
            {                
                float holdDuration = Time.time - jumpStartTime;
                
                // 0에서 1 사이의 비율 계산 (짧게 누를수록 0에 가까움)
                float holdRatio = Mathf.Clamp01(holdDuration / maxHoldTime);

                //우선 상승속도를 크게 죽여버림
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocityX, _rigid.linearVelocityY * holdRatio);

                // 보간법(Lerp)을 사용하여 짧게 누를수록(0) 높은 중력, 길게 누를수록(1) 낮은 중력을 할당
                float newGravity = Mathf.Lerp(maxGravityOnRelease, minGravityOnRelease, holdRatio);
                
                _rigid.gravityScale = newGravity;
            }

            // 2. 착지 시 또는 하강 시작 시 중력 정상화 (바닥 체크 로직 추가 필요)
            if (_rigid.linearVelocityY <= 0)
            {
                // 하강 시에는 다시 기본 중력 적용
                _rigid.gravityScale = defaultGravity; 
            }
        }

        private void CheckPlayerState()
        {
            // 1) 이번 물리 스텝에서 충돌 콜백(Enter/Stay)이 오면 아래 값들이 채워짐
            // 2) 물리 스텝 끝에서 "딱 한번" 상태를 결정하고 Controller에 전달
            if (Controller == null)
            {
                ResetStepFlags();
                return;
            }

            CheckGroundStatus();

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

                Debug.Log(i.ToString() + "번째 접촉면 벡터: " + n);

                // 바닥 판정: 노멀의 y가 충분히 위를 향하면(경사 포함) grounded
                // if (n.y > 0.5f)
                //     _groundedThisStep = true;

                // 벽 판정: 노멀의 x가 거의 좌/우를 향하면 벽
                if (Mathf.Abs(n.x) > 0.9f)
                {
                    _wallThisStep = true;
                    _wallNormalX = n.x; // 마지막으로 감지된 벽 방향 저장
                }
            }
        }

        private void CheckGroundStatus()
        {
            float extraHeight = 0.1f; // 아래로 쏘는 거리
            float inset = 0.05f;      // 양옆을 깎아내는 거리

            // 기존 사이즈보다 양옆으로 inset만큼 줄인 사이즈
            Vector2 size = new Vector2(_box2D.bounds.size.x - (inset * 2f), 0.1f);
            
            // 박스 캐스트 위치 (발밑)
            Vector2 origin = new Vector2(_box2D.bounds.center.x, _box2D.bounds.min.y);

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, extraHeight);

            if(hit.collider != null)
            {
                _groundedThisStep = true;
            }
            else
            {
                _groundedThisStep = false;
            }            
        }

        protected override bool IsAutoShow()
        {
            return true;
        }
    }
}
