using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 기존 보스 컷씬을 그대로 재생하는 어댑터. 보스 컷씬 3종은 아직 이관하지 않고 병행 유지한다.
///
/// 스킵할 수 없다. 기존 컷씬이 자기 finally에서 InputHandler의 차단 짝을 스스로 관리하므로,
/// 중간에 끊으면 이 프레임워크의 리스와 뒤엉킨다.
/// </summary>
[Serializable]
[Preserve]
public class CutsceneBossCinematicStep : CutsceneStepBase
{
    [SerializeField]
    private BossCinematicBase _cinematic;

    [SerializeField]
    private BossBase _boss;

    public override bool IsSkippable => false;

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (_cinematic == null)
        {
            return;
        }

        await _cinematic.PlayCinematicAsync(_boss, cancellationToken);

        // 기존 컷씬이 finally에서 IsFollowingPlayer를 true로 되돌린다. 곧바로 되찾는다.
        context.CameraLease.Reassert();
    }

    // 기존 컷씬은 자기 finally에서 입력과 카메라를 스스로 정리한다. 덧붙일 최종 상태가 없다.
    public override void ApplyFinalState(CutsceneContext context)
    {
    }
}
