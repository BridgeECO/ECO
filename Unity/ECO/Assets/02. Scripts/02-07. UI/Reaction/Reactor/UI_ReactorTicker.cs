using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// interactable 변화를 감시할 Reactor만 모아 프레임당 한 번 훑는다.
/// Selectable.interactable에는 변경 콜백이 없어 폴링 외에 방법이 없다.
/// </summary>
public static class UI_ReactorTicker
{
    private static readonly List<UI_Reactor> _reactors = new List<UI_Reactor>();
    private static bool _isRunning;

    public static void Register(UI_Reactor reactor)
    {
        if (reactor == null || _reactors.Contains(reactor))
        {
            return;
        }

        _reactors.Add(reactor);
        StartLoop();
    }

    public static void Unregister(UI_Reactor reactor)
    {
        int index = _reactors.IndexOf(reactor);
        if (index < 0)
        {
            return;
        }

        // 순서가 의미 없는 목록이라 마지막 항목으로 덮어써 이동 비용을 없앤다.
        int last = _reactors.Count - 1;
        _reactors[index] = _reactors[last];
        _reactors.RemoveAt(last);
    }

    // Enter Play Mode Options로 도메인 리로드를 끄면 static 상태가 플레이 세션을 넘어 남는다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _reactors.Clear();
        _isRunning = false;
    }

    private static void StartLoop()
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        RunAsync().Forget();
    }

    private static async UniTaskVoid RunAsync()
    {
        while (0 < _reactors.Count)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            Tick();
        }

        _isRunning = false;
    }

    private static void Tick()
    {
        for (int i = _reactors.Count - 1; 0 <= i; i--)
        {
            UI_Reactor reactor = _reactors[i];
            if (reactor == null)
            {
                _reactors.RemoveAt(i);
                continue;
            }

            reactor.TickInteractable();
        }
    }
}
