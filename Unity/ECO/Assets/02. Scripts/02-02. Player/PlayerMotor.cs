using System;
using UnityEngine;
using VInspector;

public class PlayerMotor : MonoBehaviour
{
    // 텔레포트 직후 후처리(FSM 상태 초기화 등)를 구독자에게 맡긴다.
    // Motor가 상위 컴포넌트(PlayerStateMachine)를 역참조하지 않기 위한 이벤트다.
    public Action OnTeleported;

    [Foldout("Project")]
    [SerializeField]
    private PhysicsMaterial2D _frictionlessMaterial;

    public Vector2 Velocity { get; private set; }
    public Vector2 ExternalVelocity { get; set; }

    // 외부 요인이 거는 추가 속도 보정. ExternalVelocity는 TerrainRiderSynchronizer가 대입해 쓰는
    // 채널이라, 공유하면 이동 발판 위에서 점프할 때 보정까지 같이 지워진다. 그래서 채널을 나눈다.
    public float SpeedCorrectionX { get; private set; }

    // 스폰 시 플레이어는 오른쪽(Quaternion.identity)을 바라보므로 true로 초기화
    public bool IsForward { get; private set; } = true;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        // 속도를 물리 프레임마다 직접 대입하므로 마찰이 할 일이 없다. 오히려 공중에서 벽 쪽으로 입력하면
        // 벽과의 마찰이 상승 속도를 깎아 점프가 낮아지므로, 상태와 무관하게 마찰 없는 재질을 유지한다.
        _rigidbody.sharedMaterial = _frictionlessMaterial;
    }

    private void FixedUpdate()
    {
        Vector2 totalVelocity = Velocity + ExternalVelocity;
        totalVelocity.x += SpeedCorrectionX;
        _rigidbody.linearVelocity = totalVelocity;
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        Velocity = newVelocity;
    }

    public void SetVelocityX(float x)
    {
        Velocity = new Vector2(x, Velocity.y);
    }

    public void SetVelocityY(float y)
    {
        Velocity = new Vector2(Velocity.x, y);
    }

    public void SetSpeedCorrectionX(float x)
    {
        SpeedCorrectionX = x;
    }

    // IL2CPP 빌드 최적화에서 Quaternion.Euler()의 부동소수점 정밀도 차이로 인해
    // 매 프레임 == 비교가 false가 되는 문제를 방지하기 위해 캐싱
    private static readonly Quaternion ROTATION_LEFT = Quaternion.Euler(0f, 180f, 0f);

    public void SetFlip(float xInput)
    {
        if (xInput == 0f)
        {
            return;
        }

        bool wantsForward = 0f < xInput;
        if (IsForward == wantsForward)
        {
            return;
        }

        transform.rotation = wantsForward ? Quaternion.identity : ROTATION_LEFT;
        IsForward = wantsForward;
    }

    public void AddVelocity(Vector2 addedVelocity)
    {
        Velocity += addedVelocity;
    }

    public void Lift(float height)
    {
        _rigidbody.position += new Vector2(0f, height);
    }

    public void Teleport(Vector2 position)
    {
        _rigidbody.position = position;
        Velocity = Vector2.zero;
        SpeedCorrectionX = 0f;
        _rigidbody.linearVelocity = Vector2.zero;
        OnTeleported?.Invoke();
    }
}