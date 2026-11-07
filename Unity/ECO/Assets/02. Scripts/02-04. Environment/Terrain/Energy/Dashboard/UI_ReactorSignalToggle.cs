using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 리액터 신호 하나를 켜고 끄는 스위치. 켜면 신호를 재생하고 끄면 같은 신호를 되감는다.
/// 같은 값이 다시 와도 연출을 되풀이하지 않도록 마지막 상태를 기억한다.
/// </summary>
[Serializable]
public class UI_ReactorSignalToggle
{
    [SerializeField]
    private UI_Reactor _reactor;

    [SerializeField]
    private EUIReactionSignal _signal = EUIReactionSignal.LightOn;

    private bool _isOn;

    public void Set(bool isOn, CancellationToken cancellationToken)
    {
        if (_isOn == isOn)
        {
            return;
        }

        _isOn = isOn;
        if (isOn)
        {
            _reactor.PlaySignalAsync(_signal, cancellationToken).Forget();
            return;
        }
        _reactor.PlaySignalExitAsync(_signal, cancellationToken).Forget();
    }

    // 리액터는 비활성화될 때 기준값으로 돌아가므로, 다시 켜지면 처음부터 다시 재생하게 한다.
    public void ResetState()
    {
        _isOn = false;
    }
}
