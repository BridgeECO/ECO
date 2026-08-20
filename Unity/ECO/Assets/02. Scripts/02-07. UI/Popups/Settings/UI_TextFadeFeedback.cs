using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 설정 변경 결과를 짧게 띄웠다 지우는 안내 문구.
///
/// 나타나고 사라지는 모양은 UI_Reactor의 Show/Hide 항목이 쥔다.
/// 이 클래스는 무슨 문구를 언제 띄우고 얼마나 남겨 둘지만 정한다.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_TextFadeFeedback : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Reactor _reactor;

    [Foldout("Settings")]
    [SerializeField]
    private float _visibleDuration = 1.5f;

    private TextMeshProUGUI _textMesh;
    private CancellationTokenSource _cts;

    #region Unity Lifecycle Methods
    private void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        ResetFade();
    }

    private void OnDisable()
    {
        ResetFade();
    }
    #endregion

    #region Logic
    public void Play(string message)
    {
        ResetFade();

        if (_textMesh != null)
        {
            _textMesh.text = message;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        PlayFadeAsync(_cts.Token).Forget();
    }

    // 등장과 퇴장 사이에 유지 시간이 끼는 순차 연출이라, 한 신호에 몰아넣지 않고 코드가 순서를 잡는다.
    private async UniTaskVoid PlayFadeAsync(CancellationToken cancellationToken)
    {
        if (_reactor == null)
        {
            return;
        }

        try
        {
            await _reactor.PlaySignalAsync(EUIReactionSignal.Show, cancellationToken);
            await UniTask.Delay(TimeSpan.FromSeconds(_visibleDuration), ignoreTimeScale: true,
                cancellationToken: cancellationToken);
            await _reactor.PlaySignalAsync(EUIReactionSignal.Hide, cancellationToken);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // 취소 또는 폐기 시 예외를 무시하고 안전하게 종료한다.
        }
    }

    private void ResetFade()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_textMesh == null)
        {
            return;
        }

        Color color = _textMesh.color;
        _textMesh.color = new Color(color.r, color.g, color.b, 0f);
    }
    #endregion
}
