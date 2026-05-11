using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerUnderJump
{
    private const float PLATFORM_ROTATIONALOFFSET_SWITCH_DURATION = 0.5f;

    public async UniTaskVoid ExecuteAsync(PlatformEffector2D effector, CancellationToken cancellationToken)
    {
        if (effector == null)
        {
            return;
        }

        float originalOffset = effector.rotationalOffset;
        effector.rotationalOffset = 180f;

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
