/// <summary>
/// 컷씬을 이미 봤는지 기억한다.
///
/// Room.ResetRoom은 "세이브 시점 상태로 되돌린다"는 의미이므로, 마지막 세이브 이후에 본
/// 컷씬은 사망 후 리스폰하면 다시 나온다. TutorialProgressState, NPCSpecialEventQueue와 같은 규칙이다.
/// </summary>
public class CutsceneProgressState
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
