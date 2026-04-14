public abstract class TerrainGimmickBase
{
    private EGimmickActivationType _activationType;
    private bool _isInverted;

    public EGimmickActivationType ActivationType => _activationType;
    public bool IsInverted => _isInverted;

    protected TerrainGimmickBase(EGimmickActivationType activationType, bool isInverted)
    {
        _activationType = activationType;
        _isInverted = isInverted;
    }

    public void Evaluate(TerrainObject target, bool isEnergyActive)
    {
        bool isActivated = (_activationType == EGimmickActivationType.Always) || isEnergyActive;
        isActivated = _isInverted ? !isActivated : isActivated;
        ApplyGimmick(target, isActivated);
    }

    protected abstract void ApplyGimmick(TerrainObject target, bool isActivated);
}
