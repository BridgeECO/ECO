using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TitleScene : MonoBehaviour
{
    // UI가 구독할 이벤트들
    public Action OnIntroStarted;
    public Action OnWaitInputStarted;
    public Action OnMenuStarted;

    public readonly float IntroDuration = 5f;

    private ETitleState _currentState = ETitleState.Intro;

    private void Start()
    {
        PlayIntroSequence().Forget();
    }

    private void Update()
    {
        if (InputHandler.IsInputBlocked)
        {
            return;
        }

        if (_currentState == ETitleState.WaitInput && Input.anyKeyDown)
        {
            ChangeState(ETitleState.Menu);
        }
    }

    private async UniTaskVoid PlayIntroSequence()
    {
        OnIntroStarted?.Invoke();
        await UniTask.Delay(TimeSpan.FromSeconds(IntroDuration),

        cancellationToken: this.GetCancellationTokenOnDestroy());
        ChangeState(ETitleState.WaitInput);
    }

    private void ChangeState(ETitleState newState)
    {
        _currentState = newState;
        switch (newState)
        {
            case ETitleState.WaitInput:
                {
                    OnWaitInputStarted?.Invoke();
                    break;
                }
            case ETitleState.Menu:
                {
                    OnMenuStarted?.Invoke();
                    break;
                }
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
