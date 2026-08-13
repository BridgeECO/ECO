using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VInspector;

public class StartChasingCinematic : BossCinematicBase
{
    [Foldout("Cinematic Settings")]
    [SerializeField]
    [Tooltip("보스에게 카메라가 이동하는 시간")]
    private float _panToBossDuration;
    [SerializeField]
    [Tooltip("카메라를 흔드는 시간")]
    private float _shakeDuration;
    [SerializeField]
    [Tooltip("포효 시 카메라 흔들림 강도")]
    private float _shakeStrength;

    [Foldout("Energy Settings")]
    [SerializeField]
    [Tooltip("컷신 도중 활성화할 에너지 라인들")]
    private List<EnergyLine> _connectedLines;

    public override async UniTask PlayCinematicAsync(BossBase boss, CancellationToken cancellationToken)
    {
        if (boss == null || _camController == null || _camEffect == null)
        {
            return;
        }

        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            this.GetCancellationTokenOnDestroy());
        CancellationToken linkedToken = linkedCts.Token;

        try
        {
            InputHandler.BlockInput();

            _camController.IsFollowingPlayer = false;

            Vector3 bossTargetPos = _camController.GetClampedPosition(boss.transform.position);
            await _camController.PanToPositionAsync(
                bossTargetPos,
                _panToBossDuration,
                Ease.InOutCubic,
                linkedToken);

            if (boss == null)
            {
                return;
            }

            boss.PlayShoutSfx();

            ActivateEnergyLines();

            await _camEffect.ShakeCameraAsync(
                _shakeDuration,
                _shakeStrength,
                cancellationToken: linkedToken);

            if (boss == null)
            {
                return;
            }

            boss.StartChase();
            await boss.WaitForStateAsync(EBossState.Chasing, linkedToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            InputHandler.UnblockInput();
            if (_camController != null)
            {
                _camController.IsFollowingPlayer = true;
            }
        }
    }
    private void ActivateEnergyLines()
    {
        foreach (EnergyLine line in _connectedLines)
        {
            if (line != null)
            {
                line.SetSwitchState(true);
            }
        }
    }
}
