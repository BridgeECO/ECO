using UnityEngine;

/// <summary>
/// 컨베이어 벨트 지형 기믹. 지형에 탑승자 감지 컴포넌트를 붙이고 에너지 상태에 따라 켜고 끄기만 한다.
/// 어느 면이 어느 쪽으로 미는지는 감지 컴포넌트가 가진 면 판정이 정한다.
/// </summary>
public class ConveyorBeltGimmick : TerrainGimmickBase
{
    private EConveyorBeltDirection _direction;
    private float _speed;
    private ConveyorBeltRiderDetector _riderDetector;

    public ConveyorBeltGimmick(EGimmickActivationType activationType, bool isInverted,
        EConveyorBeltDirection direction, float speed)
        : base(activationType, isInverted)
    {
        _direction = direction;
        _speed = speed;
    }

    // 방 초기화는 보정만 걷고 탑승자는 남긴다. 같은 벨트 위로 리스폰하면 발과 벨트의 접촉이 끊기지 않아 Enter가 다시 오지 않으므로,
    // 탑승자를 지우면 다시 밀지 못한다. 멀리 옮겨졌으면 다음 물리 단계의 Exit가 탑승자를 놓는다.
    public override void ResetGimmick(TerrainObject target)
    {
        base.ResetGimmick(target);
        if (_riderDetector != null)
        {
            _riderDetector.RemoveCorrection();
        }
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        EnsureRiderDetector(target);
        _riderDetector.SetBeltActive(isActivated);
    }

    // 충돌 콜백은 MonoBehaviour만 받으므로, 벨트 지형에만 감지 컴포넌트를 런타임에 붙인다.
    private void EnsureRiderDetector(TerrainObject target)
    {
        if (_riderDetector != null)
        {
            return;
        }

        _riderDetector = target.GetComponent<ConveyorBeltRiderDetector>();
        if (_riderDetector == null)
        {
            _riderDetector = target.gameObject.AddComponent<ConveyorBeltRiderDetector>();
        }
        _riderDetector.InitBelt(_direction, _speed);
    }
}
