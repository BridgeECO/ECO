using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어를 제자리에 세운다.
///
/// 입력 차단만으로는 서지 않는다. 지상 수평 이동은 PlayerGroundedState가 매 프레임 직접
/// 대입해 곧바로 멈추지만, Dash는 입력과 무관하게 속도를 재대입하고, 공중이면 중력이 적분되며,
/// 플레이어를 태운 지형은 매 물리 스텝 ExternalVelocity를 덮어쓴다.
/// </summary>
public class CutsceneActorFreeze
{
    private PlayerStateMachine _player;
    private bool _isHeld;

    public bool IsHeld => _isHeld;

    public async UniTask FreezeAsync(CutsceneContext context, bool isWaitForGrounded, float groundWaitTimeout,
        CancellationToken cancellationToken)
    {
        PlayerStateMachine player = context.Player;
        if (player == null)
        {
            return;
        }

        _player = player;
        _isHeld = true;

        // 착지를 기다리기 전에 Dash와 Hover를 먼저 끊는다.
        // (SetCutsceneFrozen도 안에서 같은 일을 하지만, 그때는 이미 기다림이 끝난 뒤다) 그러지 않으면 그 상태의 자체 타이머가
        // 끝날 때까지 낙하가 시작되지 않아, 기다림이 통째로 타임아웃되고 공중에 뜬 채 얼어붙는다.
        player.NormalizeStateForFreeze();

        if (isWaitForGrounded)
        {
            await WaitForGroundedAsync(player, groundWaitTimeout, cancellationToken);
        }

        player.SetCutsceneFrozen(true);
    }

    /// <summary>기다리지 않고 즉시 세운다. 스킵 경로처럼 동기로 끝나야 하는 자리에서 쓴다.</summary>
    public void Freeze(CutsceneContext context)
    {
        PlayerStateMachine player = context.Player;
        if (player == null)
        {
            return;
        }

        _player = player;
        _isHeld = true;
        player.SetCutsceneFrozen(true);
    }

    /// <summary>몇 번 불려도 안전하다. 정리 경로가 여럿이라 멱등해야 한다.</summary>
    public void Release()
    {
        if (!_isHeld)
        {
            return;
        }

        _isHeld = false;

        if (_player != null)
        {
            _player.SetCutsceneFrozen(false);
        }

        _player = null;
    }

    private static async UniTask WaitForGroundedAsync(PlayerStateMachine player, float timeout,
        CancellationToken cancellationToken)
    {
        float remaining = Mathf.Max(0f, timeout);

        while (0f < remaining)
        {
            if (player == null || player.Sensor.IsOnGround)
            {
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
            remaining -= Time.fixedDeltaTime;
        }
    }
}
