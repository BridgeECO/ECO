using UnityEngine;

public abstract class TerrainGimmickBase
{
    private EGimmickActivationType _activationType;
    private bool _isInverted;

    public EGimmickActivationType ActivationType => _activationType;
    public bool IsInverted => _isInverted;
    protected bool IsActivated { get; private set; }

    protected TerrainGimmickBase(EGimmickActivationType activationType, bool isInverted)
    {
        _activationType = activationType;
        _isInverted = isInverted;
    }

    public void Evaluate(TerrainObject target, bool isEnergyActive)
    {
        IsActivated = (_activationType == EGimmickActivationType.Always) || isEnergyActive;
        IsActivated = _isInverted ? !IsActivated : IsActivated;
        ApplyGimmick(target, IsActivated);
    }

    public virtual void OnTerrainTriggerEnter2D(Collider2D other) { }
    public virtual void OnTerrainTriggerExit2D(Collider2D other) { }
    public virtual void OnDestroy(TerrainObject target) { }

    protected abstract void ApplyGimmick(TerrainObject target, bool isActivated);
}
