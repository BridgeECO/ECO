using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_ConveyorBelt", menuName = "Scriptable Objects/Terrain Gimmick/ConveyorBeltGimmickSO")]
public class ConveyorBeltGimmickSO : TerrainGimmickBaseSO
{
    [Tooltip("벨트 둘레가 도는 방향. 윗면에 선 플레이어를 시계방향은 오른쪽, 반시계방향은 왼쪽으로 민다. 옆면은 시계방향이면 왼쪽 옆면 위, 오른쪽 옆면 아래로 민다(반시계방향은 반대).")]
    [SerializeField]
    private EConveyorBeltDirection _direction;

    [Tooltip("벨트가 미는 추가 속도. 벨트 위에서 걸을 때 더해지는 값과 점프할 때 공중으로 한 번 넘어가는 값, 옆면에 붙어 벽타기 하는 동안 세로로 더해지는 값도 이 값이다. 음수면 반대쪽으로 민다.")]
    [SerializeField]
    private float _speed;

    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new ConveyorBeltGimmick(ActivationType, IsInverted, _direction, _speed);
    }
}
