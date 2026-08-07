using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class TerrainGimmickBase
{
    private EGimmickActivationType _activationType;
    private bool _isInverted;

    public EGimmickActivationType ActivationType => _activationType;
    public bool IsInverted => _isInverted;
    protected bool IsActivated { get; private set; }

    // 김믹은 POCO라 소유 오브젝트 파괴를 스스로 감지할 수 없다.
    // 파생 김믹이 CTS를 만들 때 이 토큰과 연결해 씬 전환 시 태스크 누수를 방지한다.
    protected CancellationToken OwnerDestroyToken { get; private set; }

    protected TerrainGimmickBase(EGimmickActivationType activationType, bool isInverted)
    {
        _activationType = activationType;
        _isInverted = isInverted;
    }

    public void Evaluate(TerrainObject target, bool isEnergyActive)
    {
        OwnerDestroyToken = target.GetCancellationTokenOnDestroy();
        IsActivated = (_activationType == EGimmickActivationType.Always) || isEnergyActive;
        IsActivated = _isInverted ? !IsActivated : IsActivated;
        ApplyGimmick(target, IsActivated);
    }

    public virtual void OnTerrainTriggerEnter2D(Collider2D other) { }
    public virtual void OnTerrainTriggerExit2D(Collider2D other) { }
    public virtual void OnDestroy(TerrainObject target) { }
    public virtual void ResetGimmick(TerrainObject target) { }

    protected abstract void ApplyGimmick(TerrainObject target, bool isActivated);
}
