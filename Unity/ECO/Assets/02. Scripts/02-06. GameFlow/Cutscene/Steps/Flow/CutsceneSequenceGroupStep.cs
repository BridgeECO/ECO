using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

/// <summary>자식들을 위에서 아래로 하나씩 재생한다. 동시 실행 그룹 안의 한 갈래를 여러 단계로 만들 때 쓴다.</summary>
[Serializable]
[Preserve]
public class CutsceneSequenceGroupStep : CutsceneGroupStepBase
{
    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            // DOTween을 쓰는 스텝은 취소돼도 예외 없이 정상 완료한다(UniTask의 기본 TweenCancelBehaviour.Kill).
            // 매 반복 직접 확인하지 않으면 취소된 뒤에도 다음 자식을 실제로 재생한다.
            cancellationToken.ThrowIfCancellationRequested();

            CutsceneStepBase child = Steps[i];
            if (ReferenceEquals(child, null) || !child.IsEnabled)
            {
                continue;
            }

            await child.PlayAsync(context, cancellationToken);
        }
    }
}
