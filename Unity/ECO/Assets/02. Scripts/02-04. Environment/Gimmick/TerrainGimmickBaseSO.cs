using UnityEngine;

public abstract class TerrainGimmickBaseSO : ScriptableObject
{
    [SerializeField]
    private EGimmickActivationType _activationType;

    [SerializeField]
    private bool _isInverted;

    public EGimmickActivationType ActivationType => _activationType;
    public bool IsInverted => _isInverted;

    public abstract TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry);
}
