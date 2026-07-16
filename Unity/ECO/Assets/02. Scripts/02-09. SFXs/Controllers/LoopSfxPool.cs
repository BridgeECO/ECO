using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESfxClip 키 기반 루프 AudioSource 풀.
/// 루프 소스의 생성·재생·정지·볼륨 제어를 단일 책임으로 담당한다.
/// SfxController의 자식 오브젝트에 배치되는 내부 컴포넌트이며, 외부에서 직접 참조하지 않는다.
/// </summary>
public class LoopSfxPool : MonoBehaviour
{
    private readonly Dictionary<int, AudioSource> _sources = new();
    private float _volume = 1f;

    public void Play(ESfxClip clip, AudioClip audioClip)
    {
        int key = (int)clip;
        if (_sources.TryGetValue(key, out var source))
        {
            if (source != null)
            {
                if (!source.isPlaying)
                {
                    source.clip = audioClip;
                    source.Play();
                }
                return;
            }
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.spatialBlend = 0f;
        newSource.loop = true;
        newSource.playOnAwake = false;
        newSource.volume = _volume;
        newSource.clip = audioClip;
        newSource.Play();

        _sources[key] = newSource;
    }

    public void Stop(ESfxClip clip)
    {
        int key = (int)clip;
        if (_sources.TryGetValue(key, out var source) && source != null)
        {
            source.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        _volume = volume;
        foreach (var source in _sources.Values)
        {
            if (source != null)
            {
                source.volume = _volume;
            }
        }
    }

    private void OnDestroy()
    {
        _sources.Clear();
    }
}
