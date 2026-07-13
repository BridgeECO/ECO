using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_TextFadeFeedback : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    private CancellationTokenSource _cts;

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

    private void ResetFade()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_textMesh != null)
        {
            Color color = _textMesh.color;
            _textMesh.color = new Color(color.r, color.g, color.b, 0f);
        }
    }

    private async UniTaskVoid PlayFadeAsync(CancellationToken cancellationToken)
    {
        if (_textMesh == null)
        {
            return;
        }

        Color originalColor = _textMesh.color;

        // 1. Fade In (0.1s)
        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            _textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
        _textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        // 2. Keep Active (1.5s)
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.5f), cancellationToken: cancellationToken);

        // 3. Fade Out (0.1s)
        elapsed = 0f;
        duration = 0.1f;
        while (elapsed < duration)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            _textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
        _textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}
