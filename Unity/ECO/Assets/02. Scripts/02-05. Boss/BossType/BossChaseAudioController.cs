using UnityEngine;

public sealed class BossChaseAudioController
{
    private readonly BossDataSO _bossData;
    private readonly AudioSource _sfxAudioSource;
    private readonly AudioSource _sfxLoopAudioSource;
    private ESfxClip? _currentChaseSound;

    public BossChaseAudioController(
        BossDataSO bossData,
        AudioSource sfxAudioSource,
        AudioSource sfxLoopAudioSource)
    {
        _bossData = bossData;
        _sfxAudioSource = sfxAudioSource;
        _sfxLoopAudioSource = sfxLoopAudioSource;
    }

    public void RefreshChase(float currentSpeed)
    {
        ESfxClip sound = currentSpeed == _bossData.BaseSpeed
            ? ESfxClip.SE_TB_Walking
            : ESfxClip.SE_TB_Rushing;
        if (_currentChaseSound == sound)
        {
            return;
        }

        _currentChaseSound = sound;
        SoundManager.Instance.PlayLoopSfxOnSource(sound, _sfxLoopAudioSource);
    }

    public void PlayCrash()
    {
        SoundManager.Instance.PlaySfxOnSource(ESfxClip.SE_TB_Crash, _sfxAudioSource);
    }

    public void StopChase()
    {
        SoundManager.Instance.StopLoopSfxOnSource(_sfxLoopAudioSource);
        _currentChaseSound = null;
    }
}
