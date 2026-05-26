public class LifeRecoveryObject : SpecialObjectBase
{
    protected override void Interact()
    {
        base.Interact();
        LifeManager.Instance.RecoverOne();
        SetState(false);
    }

    protected override void SetState(bool isOn)
    {
        gameObject.SetActive(isOn);
    }

    public override void ResetState()
    {
        base.ResetState();
        gameObject.SetActive(true);
    }
}
