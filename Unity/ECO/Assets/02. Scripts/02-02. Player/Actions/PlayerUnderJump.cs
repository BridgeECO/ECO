using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerUnderJump
{
    private const float PLATFORM_ROTATIONALOFFSET_SWITCH_DURATION = 0.5f;
    private const float PASS_THROUGH_ROTATIONAL_OFFSET = 180f;

    // 아래 점프 중인 발판은 뒤집혀 있어 위에서 내려오는 플레이어를 통과시키므로, 딛거나 걸리는 대상에서 빼야 한다.
    public static bool IsPassingThrough(Collider2D platform)
    {
        return platform.TryGetComponent(out PlatformEffector2D effector) &&
               effector.rotationalOffset == PASS_THROUGH_ROTATIONAL_OFFSET;
    }

    public async UniTaskVoid ExecuteAsync(PlatformEffector2D effector, CancellationToken cancellationToken)
    {
        if (effector == null)
        {
            return;
        }

        float originalOffset = effector.rotationalOffset;
        effector.rotationalOffset = PASS_THROUGH_ROTATIONAL_OFFSET;

        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds
                (PLATFORM_ROTATIONALOFFSET_SWITCH_DURATION), cancellationToken: cancellationToken);
        }
        finally
        {
            if (effector != null)
            {
                effector.rotationalOffset = originalOffset;
            }
        }
    }
}
