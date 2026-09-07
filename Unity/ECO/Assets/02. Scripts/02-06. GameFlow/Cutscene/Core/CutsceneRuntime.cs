using UnityEngine;

/// <summary>
/// Director가 쥐는 런타임 객체 한 벌. 셋이 늘 함께 만들어지고 함께 버려지므로 한 자리에 묶는다.
///
/// Director가 직접 세 개를 지연 생성하면 그 보일러플레이트만으로 클래스가 부풀고,
/// 셋의 생성 순서(Session은 Context를 필요로 한다)가 Director에 새어 나온다.
/// </summary>
public class CutsceneRuntime
{
    public CutsceneContext Context { get; }
    public CutsceneSession Session { get; }
    public CutsceneProgressState Progress { get; }

    public CutsceneRuntime(Transform owner)
    {
        Context = new CutsceneContext(owner);
        Session = new CutsceneSession(Context);
        Progress = new CutsceneProgressState();
    }
}
