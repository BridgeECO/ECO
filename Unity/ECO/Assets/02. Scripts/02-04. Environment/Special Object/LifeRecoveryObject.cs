public class LifeRecoveryObject : SpecialObjectBase
{
    protected override void Interact()
    {
        base.Interact();
        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.PlayPlayerSfx(ESfxClip.SE_SO_HealingObject);
        }
        LifeManager.Instance.Recover();
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
