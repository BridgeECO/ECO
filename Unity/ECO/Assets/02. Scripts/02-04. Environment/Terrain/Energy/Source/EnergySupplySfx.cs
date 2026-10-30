using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 에너지가 선을 따라 퍼지거나 빠지는 동안에만 공급음과 차단음을 낸다.
/// 두 클립 모두 16초 남짓한 지속음이라, 한 번 재생하고 두면 흐름이 끝난 뒤에도 울리고 공급과 차단이 겹친다.
/// 그래서 에너지원의 AudioSource에 반복 재생하다가, 연결된 선의 흐름이 모두 끝나면 멈춘다.
/// </summary>
public class EnergySupplySfx
{
    private IReadOnlyList<EnergyLine> _lines;
    private AudioSource _audioSource;
    private bool _isPlaying;
    private bool _isFeeding;

    public void Init(IReadOnlyList<EnergyLine> lines, AudioSource audioSource)
    {
        _lines = lines;
        _audioSource = audioSource;
    }

    public void Play(bool isFeeding)
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }

        _isPlaying = true;
        _isFeeding = isFeeding;
        ESfxClip clip = isFeeding ? ESfxClip.SE_Energy_Feeding : ESfxClip.SE_Energy_Vanishing;
        SoundManager.Instance.PlayLoopSfxOnSource(clip, _audioSource);
    }

    public void Stop()
    {
        if (!_isPlaying)
        {
            return;
        }

        _isPlaying = false;
        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.StopLoopSfxOnSource(_audioSource);
        }
    }

    public void Refresh()
    {
        if (_isPlaying && !IsAnyLineSpreading())
        {
            Stop();
        }
    }

    // 공급음은 가장 최근 펄스의 머리가 선 끝에 닿을 때까지, 차단음은 끊긴 펄스가 선에서 모두 빠질 때까지 이어진다.
    private bool IsAnyLineSpreading()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            EnergyLine line = _lines[i];
            if (line == null || line.Segments.Count == 0)
            {
                continue;
            }

            EnergySegment newest = line.Segments[line.Segments.Count - 1];
            bool isSpreading = _isFeeding
                ? !newest.IsCuttingOff && newest.HeadDistance < line.TotalDistance
                : newest.IsCuttingOff;
            if (isSpreading)
            {
                return true;
            }
        }
        return false;
    }
}
