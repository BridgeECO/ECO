using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VInspector;

/// <summary>
/// BGM 재생 및 트랙 전환을 담당한다.
/// 두 개의 AudioSource를 교차 사용하는 크로스페이드 방식으로 자연스러운 BGM 전환을 구현한다.
/// SoundManager의 자식 오브젝트에 배치되는 내부 컴포넌트이며, 외부에서 직접 참조하지 않는다.
/// </summary>
public class BgmController : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private AudioSource _primarySource;

    [SerializeField]
    private AudioSource _secondarySource;

    private float _volume = 1f;

    private AudioSource _currentSource;
    private AudioSource _nextSource;
    private CancellationTokenSource _fadeCts;

    private void Awake()
    {
        _currentSource = _primarySource;
        _nextSource = _secondarySource;
        InitAudioSource(_currentSource);
        InitAudioSource(_nextSource);
    }

    private void OnDestroy()
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
    }

    public void Play(AudioClip clip, float fadeDuration)
    {
        CancelFade();
        _fadeCts = new CancellationTokenSource();
        CrossfadeAsync(clip, fadeDuration, _fadeCts.Token).Forget();
    }

    public void Stop(float fadeDuration)
    {
        CancelFade();
        _fadeCts = new CancellationTokenSource();
        FadeOutAsync(fadeDuration, _fadeCts.Token).Forget();
    }

    public void SetVolume(float volume)
    {
        _volume = volume;

        // 페이드 중이 아닐 때 즉시 현재 소스에 반영
        if (!ReferenceEquals(_fadeCts, null))
        {
            return;
        }

        if (_currentSource.isPlaying)
        {
            _currentSource.volume = _volume;
        }
    }

    private void InitAudioSource(AudioSource source)
    {
        source.loop = true;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.volume = 0f;
    }

    private void CancelFade()
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = null;
    }

    private async UniTaskVoid CrossfadeAsync(AudioClip clip, float fadeDuration, CancellationToken token)
    {
        _nextSource.clip = clip;
        _nextSource.volume = 0f;
        _nextSource.Play();

        float elapsed = 0f;
        float startVolume = _currentSource.volume;

        while (elapsed < fadeDuration)
        {
            bool cancelled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            if (cancelled)
            {
                return;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            _currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
            _nextSource.volume = Mathf.Lerp(0f, _volume, t);
        }

        _currentSource.Stop();
        _currentSource.volume = 0f;
        _nextSource.volume = _volume;

        // 소스 역할 교체: 새 트랙이 current가 됨
        (_currentSource, _nextSource) = (_nextSource, _currentSource);
        _fadeCts = null;
    }

    private async UniTaskVoid FadeOutAsync(float fadeDuration, CancellationToken token)
    {
        float elapsed = 0f;
        float startVolume = _currentSource.volume;

        while (elapsed < fadeDuration)
        {
            bool cancelled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            if (cancelled)
            {
                return;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            _currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
        }

        _currentSource.Stop();
        _currentSource.volume = 0f;
        _fadeCts = null;
    }
}
