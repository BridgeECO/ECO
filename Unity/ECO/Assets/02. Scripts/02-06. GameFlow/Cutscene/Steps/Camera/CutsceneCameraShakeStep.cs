using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 카메라를 흔든다. 흔들림은 자식 카메라의 localPosition을 쓰므로 루트를 옮기는 이동 스텝과
/// 채널이 달라, 동시 실행 그룹에 나란히 넣어도 서로를 덮지 않는다.
/// </summary>
[Serializable]
[Preserve]
public class CutsceneCameraShakeStep : CutsceneStepBase
{
    [SerializeField]
    [Min(0f)]
    private float _duration = 0.5f;

    [SerializeField]
    [Min(0f)]
    private float _strength = 0.4f;

    [SerializeField]
    [Min(1)]
    private int _vibrato = 10;

    [SerializeField]
    [Tooltip("해제하면 흔들기를 시작만 하고 다음 스텝으로 넘어갑니다.")]
    private bool _isWaitForEnd = true;

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (!context.TryResolveCamera())
        {
            return;
        }

        UniTask shakeTask = context.CameraMover.ShakeAsync(context, _duration, _strength, _vibrato, cancellationToken);
        if (!_isWaitForEnd)
        {
            shakeTask.Forget();
            return;
        }

        await shakeTask;
    }

    // 흔들기는 취소돼도 자기 finally에서 localPosition을 0으로 되돌린다. 남는 오프셋이 없다.
    public override void ApplyFinalState(CutsceneContext context)
    {
    }
}
