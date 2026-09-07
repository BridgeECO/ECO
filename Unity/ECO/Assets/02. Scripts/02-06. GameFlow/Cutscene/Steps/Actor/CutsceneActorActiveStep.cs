using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 연출 오브젝트를 켜고 끈다. 등장할 오브젝트는 씬에 비활성으로 미리 놓아 둔다.
/// (동적 생성은 PoolManager를 거쳐야 하는데, 컷씬 전용 연출을 풀에 등록할 이유가 없다)
/// </summary>
[Serializable]
[Preserve]
public class CutsceneActorActiveStep : CutsceneStepBase
{
    [SerializeField]
    private List<GameObject> _targets = new List<GameObject>();

    [SerializeField]
    private bool _isActive = true;

    public override UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        Apply();
        return UniTask.CompletedTask;
    }

    public override void ApplyFinalState(CutsceneContext context)
    {
        Apply();
    }

    private void Apply()
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            GameObject target = _targets[i];
            if (target != null)
            {
                target.SetActive(_isActive);
            }
        }
    }
}
