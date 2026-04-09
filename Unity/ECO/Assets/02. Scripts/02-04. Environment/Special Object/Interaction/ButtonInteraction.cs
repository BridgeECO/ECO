using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ButtonInteraction : InteractionBase
{
    private readonly float _deactivateDelayTime;
    private bool _isInteracting = false;

    public ButtonInteraction(SpecialObjectBase target) : base(target)
    {
        ButtonInteractionData data = target.GetComponent<ButtonInteractionData>();
        _deactivateDelayTime = data != null ? data.DeactivateDelayTime : 2f;
    }

    public override void OnUpdate()
    {
        if (_isInteracting)
        {
            return;
        }

        if (TargetObject.IsPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            InteractAsync().Forget();
        }
    }

    private async UniTaskVoid InteractAsync()
    {
        _isInteracting = true;
        TargetObject.CallInteract();

        CancellationToken token = TargetObject.GetCancellationTokenOnDestroy();
        bool isCancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(_deactivateDelayTime), cancellationToken: token).SuppressCancellationThrow();

        if (isCancelled)
        {
            return;
        }

        TargetObject.CallSetState(false);
        _isInteracting = false;
    }
}
