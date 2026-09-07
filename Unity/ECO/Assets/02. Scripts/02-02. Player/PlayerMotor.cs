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
    // 스폰 시 플레이어는 오른쪽(Quaternion.identity)을 바라보므로 true로 초기화
    public bool IsForward { get; private set; } = true;

    public bool IsFrozen => _isFrozen;

    private Rigidbody2D _rigidbody;
    private bool _isFrozen;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_frictionlessMaterial == null)
        {
            CreatePhysicsMaterial2D();
        }
    }

    private void FixedUpdate()
    {
        // 동결은 매 물리 스텝 유지돼야 한다. ExternalVelocity는 플레이어를 태운 지형이
        // FixedUpdate마다 다시 채우므로, 한 번 0을 대입하는 것만으로는 순서 경합에서 진다.
        _rigidbody.linearVelocity = _isFrozen ? Vector2.zero : Velocity + ExternalVelocity;
    }

    private void CreatePhysicsMaterial2D()
    {
        _frictionlessMaterial = new PhysicsMaterial2D();
        _frictionlessMaterial.friction = 0f;
        _frictionlessMaterial.bounciness = 0f;
    }

    public void SetFriction(bool enabled)
    {
        _rigidbody.sharedMaterial = enabled ? null : _frictionlessMaterial;
    }

    /// <summary>
    /// 컷씬 동결. 해제할 때도 속도를 0으로 만든다. 동결 중에 남은 지형 속도가
    /// 해제되는 프레임에 그대로 튀어나가는 것을 막는다.
    /// </summary>
    public void SetFrozen(bool isFrozen)
    {
        _isFrozen = isFrozen;
        Velocity = Vector2.zero;
        ExternalVelocity = Vector2.zero;
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

    public void Teleport(Vector2 position)
    {
        _rigidbody.position = position;
        Velocity = Vector2.zero;
        _rigidbody.linearVelocity = Vector2.zero;
        OnTeleported?.Invoke();
    }
}