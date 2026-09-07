using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 컷씬을 이루는 한 동작. 기획자가 인스펙터에서 고르는 단위다.
///
/// 파생 클래스 파일 하나를 추가하는 것만으로 연출이 늘어나고 기존 코드는 수정하지 않는다.
/// 클래스명을 바꾸면 저장된 SerializeReference 참조가 끊기므로 리네임할 때는
/// [MovedFrom]에 이전 이름을 남긴다. IL2CPP 스트리핑에 대비해 파생마다 [Preserve]를 단다.
/// </summary>
[Serializable]
public abstract class CutsceneStepBase
{
    // 리스트에 원소를 추가하면 유니티가 C# 초기값을 무시하고 0으로 채운다. 새 필드도 0이 정상 동작이어야 한다.
    [SerializeField]
    [Tooltip("인스펙터에서 항목을 구분하기 위한 이름입니다. 동작에는 영향이 없습니다.")]
    private string _label = string.Empty;

    [SerializeField]
    [Tooltip("체크하면 이 스텝을 건너뜁니다.")]
    private bool _isMuted = false;

    public string Label => _label;

    public bool IsEnabled => !_isMuted;

    /// <summary>
    /// 중간에 끊어도 되는지. "기획이 스킵을 허용하는가"가 아니라 "끊기면 망가지는가"라는
    /// 기술 판단이라 인스펙터에 노출하지 않고 코드가 선언한다.
    /// </summary>
    public virtual bool IsSkippable => true;

    public abstract UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken);

    /// <summary>
    /// 건너뛰었을 때 화면에 남아야 할 결과를 즉시 만든다. 동기·멱등·최종 상태만.
    ///
    /// virtual이 아니라 abstract인 것은 의도다. 스텝을 새로 만드는 사람이
    /// "이 연출을 건너뛰면 무엇이 남아야 하는가"에 반드시 한 번 답하게 한다.
    /// 이 프레임워크에서 가장 먼저 썩는 코드 경로다.
    /// </summary>
    public abstract void ApplyFinalState(CutsceneContext context);
}
