/// <summary>
/// 튜토리얼의 발동 여부와 세이브 시점 스냅샷을 관리한다.
/// Room.ResetRoom은 "세이브 시점 상태로 되돌린다"는 의미이므로, 마지막 세이브 이후에
/// 본 튜토리얼은 사망 후 리스폰하면 다시 나타난다. (NPCSpecialEventQueue와 같은 규칙)
/// </summary>
public class TutorialProgressState
{
    private bool _isFired;
    private bool _isSavedFired;

    public bool IsFired => _isFired;

    public void MarkFired()
    {
        _isFired = true;
    }

    public void SaveFiredState()
    {
        _isSavedFired = _isFired;
    }

    public void ResetToSavedState()
    {
        _isFired = _isSavedFired;
    }
}
