using System;
using UnityEngine;

/// <summary>
/// 벨트에 닿은 탑승자를 추적한다. 윗면을 보는 발과 옆면을 보는 몸통을 따로 기억하지만, 같은 리지드바디의 두 콜라이더라 한 탑승자다.
/// 탑승자가 텔레포트하면 알린다. 어느 면으로 보정을 걸지는 감지 컴포넌트가 정한다.
/// </summary>
public class ConveyorBeltRider
{
    public Action OnRiderTeleported;

    public Collider2D Feet { get; private set; }
    public Collider2D Body { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerSpeedCorrector Corrector { get; private set; }
    public bool HasRider => Feet != null || Body != null;

    // 발은 윗면, 몸통은 옆면을 본다. 미끄러짐 콜라이더(Untagged)는 무시하고, 이미 추적 중인 콜라이더가 다시 닿은 것도 무시한다.
    public void TrackContact(Collider2D other)
    {
        if (Feet == null && other.CompareTag(nameof(ETags.PlayerFeet)) && TryGetOwner(other, out PlayerStateMachine feetOwner))
        {
            TrackFeet(other, feetOwner);
        }
        else if (Body == null && other.CompareTag(nameof(ETags.Player)) && TryGetOwner(other, out PlayerStateMachine bodyOwner))
        {
            BindStateMachine(bodyOwner);
            Body = other;
        }
    }

    public void TrackFeet(Collider2D feet, PlayerStateMachine stateMachine)
    {
        BindStateMachine(stateMachine);
        Feet = feet;
    }

    public bool TryGetOwner(Collider2D riderCollider, out PlayerStateMachine stateMachine)
    {
        stateMachine = null;
        Rigidbody2D riderRigidbody = riderCollider.attachedRigidbody;
        return riderRigidbody != null && riderRigidbody.TryGetComponent(out stateMachine);
    }

    public void ForgetFeet()
    {
        Feet = null;
    }

    public void ForgetBody()
    {
        Body = null;
    }

    // 벨트에서 멀어진 콜라이더를 잊고, 아직 남은 콜라이더가 있는지 돌려준다. 발은 접지 판정이 발 아래에서 이 벨트를 보는 동안 남긴다.
    public bool TryKeepTouching(Collider2D belt, bool isBeltBelow)
    {
        if (Feet != null && !isBeltBelow && !Feet.IsTouching(belt))
        {
            Feet = null;
        }
        if (Body != null && !Body.IsTouching(belt))
        {
            Body = null;
        }
        return HasRider;
    }

    public void Release()
    {
        if (StateMachine != null)
        {
            StateMachine.Motor.OnTeleported -= HandleTeleported;
        }
        Feet = null;
        Body = null;
        StateMachine = null;
        Corrector = null;
    }

    // 발과 몸통 중 먼저 닿은 쪽이 텔레포트를 구독한다. 둘째 콜라이더가 닿아도 같은 탑승자라 다시 구독하지 않는다.
    private void BindStateMachine(PlayerStateMachine stateMachine)
    {
        if (StateMachine != null)
        {
            return;
        }

        StateMachine = stateMachine;
        Corrector = stateMachine.SpeedCorrector;
        StateMachine.Motor.OnTeleported += HandleTeleported;
    }

    private void HandleTeleported()
    {
        OnRiderTeleported?.Invoke();
    }
}
