using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 인스펙터에서 자유롭게 추가·삭제하는 UI 연출 한 건. 파생 클래스를 만들면 드롭다운에 자동으로 나타난다.
/// </summary>
// [SerializeReference]로 직렬화되므로 클래스명·네임스페이스·어셈블리가 바뀌면 저장된 참조가 끊긴다.
// 리네임 시 [MovedFrom]을, IL2CPP 스트리핑 대비로 파생 클래스마다 [Preserve]를 붙인다(상속되지 않는다).
[Serializable]
public abstract class UI_ReactionBase
{
    [SerializeField]
    [Tooltip("비워 두면 UI_Reactor가 붙은 오브젝트에 적용됩니다.")]
    private GameObject _target;

    /// <summary>같은 대상의 같은 채널을 노리는 리액션끼리만 경합한다.</summary>
    public abstract EUIReactionChannel Channel { get; }

    public abstract bool IsPlaying { get; }

    public virtual GameObject ResolveTarget(UI_ReactionContext context)
    {
        return _target != null ? _target : context.Owner;
    }

    public abstract UniTask PlayAsync(UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken);

    /// <summary>상태가 끝나 물러난다. 단발 트리거에는 호출되지 않는다.</summary>
    public abstract UniTask ExitAsync(UI_ReactionContext context,
        EUIReactionExitPolicy exitPolicy, CancellationToken cancellationToken);

    /// <summary>진행 중인 연출만 즉시 끊는다. 값은 건드리지 않는다.</summary>
    public abstract void Kill();

    /// <summary>
    /// 기준값을 되돌린다. OnDisable에서 호출되므로 반드시 동기로 끝나야 한다.
    /// 풀링된 UI가 같은 프레임에 재사용될 수 있어 await로 넘기면 복원이 유실된다.
    /// </summary>
    public abstract void RestoreBaseline(UI_ReactionContext context);

    /// <summary>
    /// 아무도 건드리기 전의 값을 한 번만 잡아 두고 그 값을 돌려준다.
    /// 이미 잡혀 있으면 넘긴 현재 값이 아니라 저장된 값을 쓴다.
    /// </summary>
    protected UI_ReactionBaseline EnsureBaseline(UI_ReactionContext context, GameObject target,
        Vector4 value, UnityEngine.Object reference, bool flag)
    {
        int targetId = target.GetInstanceID();
        if (context.Baselines.TryGet(targetId, Channel, out UI_ReactionBaseline stored))
        {
            return stored;
        }

        UI_ReactionBaseline baseline = new UI_ReactionBaseline
        {
            TargetId = targetId,
            Channel = Channel,
            Value = value,
            Reference = reference,
            Flag = flag,
        };
        context.Baselines.Set(baseline);
        return baseline;
    }

    /// <summary>아직 아무도 이 채널을 건드리지 않았다면 false. 이때는 되돌릴 것도 없다.</summary>
    protected bool TryGetBaseline(UI_ReactionContext context, GameObject target,
        out UI_ReactionBaseline baseline)
    {
        return context.Baselines.TryGet(target.GetInstanceID(), Channel, out baseline);
    }
}
