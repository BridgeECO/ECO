using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

/// <summary>자식들을 한꺼번에 시작해 전부 끝날 때까지 기다린다.</summary>
[Serializable]
[Preserve]
public class CutsceneParallelGroupStep : CutsceneGroupStepBase
{
    private List<UniTask> _tasks;

    // [SerializeReference] 역직렬화 경로에서 필드 이니셜라이저가 도는 보장이 없어 지연 생성한다.
    private List<UniTask> Tasks
    {
        get
        {
            if (_tasks == null)
            {
                _tasks = new List<UniTask>();
            }

            return _tasks;
        }
    }

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        using CancellationTokenSource groupCts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        List<UniTask> tasks = Tasks;
        tasks.Clear();

        try
        {
            // UniTask는 hot이라 이 줄에서 이미 실행이 시작된다. 시작 루프를 try 밖에 두면
            // 자식 하나가 첫 await 전에 동기 예외를 던졌을 때 finally가 돌지 않아,
            // 이미 시작된 형제들이 취소되지 못한 채 컷씬이 끝난 뒤에도 계속 돈다.
            for (int i = 0; i < Steps.Count; i++)
            {
                CutsceneStepBase child = Steps[i];
                if (ReferenceEquals(child, null) || !child.IsEnabled)
                {
                    continue;
                }

                tasks.Add(child.PlayAsync(context, groupCts.Token));
            }

            await UniTask.WhenAll(tasks);
        }
        finally
        {
            // WhenAll은 첫 예외만 다시 던지고 나머지를 멈추지 않는다. 형제 정리는 이 취소가 유일한 수단이다.
            groupCts.Cancel();
            tasks.Clear();
        }
    }
}
