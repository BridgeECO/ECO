using UnityEngine;

public abstract class TerrainGimmickBaseSO : ScriptableObject
{
    [SerializeField]
    private EGimmickActivationType _activationType;

    [SerializeField]
    private bool _isInverted;

    public EGimmickActivationType ActivationType => _activationType;
    public bool IsInverted => _isInverted;

    // 지형을 실제로 이동시키는 김믹인지 여부. 소유자(TerrainObject)가 구체 SO 타입을
    // 검사하지 않고도 판별할 수 있도록 SO 계층이 스스로 답한다.
    public virtual bool IsMovementGimmick => false;

    public abstract TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry);

    public virtual void DrawGizmos(TerrainObject target, TerrainGimmickEntry entry) { }
}
