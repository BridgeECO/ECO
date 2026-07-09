using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// BGM 및 SFX AudioClip 에셋들을 카테고리별로 관리하고,
/// 런타임에 에셋명과 Enum(EBgmType, ESfxClip) 값을 1:1로 매핑하여 빠른 인덱스 기반 조회를 지원하는 에셋 라이브러리 클래스.
/// SoundManager 내부에서 직렬화(Serialization)되어 인라인으로 관리된다.
/// </summary>
[Serializable]
public class SoundLibrary
{
    [SerializeField]
    private List<AudioClip> _bgmClips;

    [Foldout("SFX Categories")]
    [SerializeField]
    private List<AudioClip> _ambientSfxClips;

    [SerializeField]
    private List<AudioClip> _playerSfxClips;

    [SerializeField]
    private List<AudioClip> _bossSfxClips;

    [SerializeField]
    private List<AudioClip> _objectSfxClips;

    [SerializeField]
    private List<AudioClip> _uiSfxClips;

    // 런타임 고속 Lookup을 위해 Enum 인덱스로 정렬된 에셋 캐싱 리스트
    private List<AudioClip> _runtimeBgmClips;
    private List<AudioClip> _runtimeSfxClips;

    // SoundManager의 Awake 시점에 호출되어 캐시 리스트를 초기화하고 무결성을 확인한다.
    public void Initialize()
    {
        InitializeBgmClips();
        InitializeSfxClips();
        ValidateClips();
    }

    // EBgmType enum 값에 해당하는 BGM AudioClip을 반환한다.
    public AudioClip GetBgmClip(EBgmType type)
    {
        int index = (int)type;
        if (_runtimeBgmClips is null || index < 0 || index >= _runtimeBgmClips.Count)
        {
            return null;
        }

        return _runtimeBgmClips[index];
    }

    // ESfxClip enum 값에 해당하는 SFX AudioClip을 반환한다.
    public AudioClip GetSfxClip(ESfxClip clip)
    {
        int index = (int)clip;
        if (_runtimeSfxClips is null || index < 0 || index >= _runtimeSfxClips.Count)
        {
            return null;
        }

        return _runtimeSfxClips[index];
    }

    // BGM 에셋 리스트를 스캔하여 EBgmType 인덱스에 맞게 런타임 캐시 리스트를 초기화한다.
    private void InitializeBgmClips()
    {
        int enumCount = Enum.GetValues(typeof(EBgmType)).Length;
        _runtimeBgmClips = new List<AudioClip>(new AudioClip[enumCount]);

        for (int i = 0; i < _bgmClips.Count && i < enumCount; i++)
        {
            _runtimeBgmClips[i] = _bgmClips[i];
        }
    }

    // 여러 카테고리로 나누어 수집된 SFX 에셋들을 취합하여 ESfxClip 인덱스 순서에 맞춘 캐시 리스트를 초기화한다.
    private void InitializeSfxClips()
    {
        int enumCount = Enum.GetValues(typeof(ESfxClip)).Length;
        _runtimeSfxClips = new List<AudioClip>(new AudioClip[enumCount]);

        AddClipsToSfxList(_ambientSfxClips);
        AddClipsToSfxList(_playerSfxClips);
        AddClipsToSfxList(_bossSfxClips);
        AddClipsToSfxList(_objectSfxClips);
        AddClipsToSfxList(_uiSfxClips);
    }

    // 원본 카테고리 리스트에서 AudioClip의 이름과 일치하는 ESfxClip의 인덱스 슬롯에 클립을 매핑한다.
    private void AddClipsToSfxList(List<AudioClip> sourceClips)
    {
        if (sourceClips is null)
        {
            return;
        }

        foreach (AudioClip clip in sourceClips)
        {
            if (clip == null)
            {
                continue;
            }

            if (Enum.TryParse<ESfxClip>(clip.name, out ESfxClip sfxClip))
            {
                int index = (int)sfxClip;
                if (index >= 0 && index < _runtimeSfxClips.Count)
                {
                    _runtimeSfxClips[index] = clip;
                }
            }
            else
            {
                Debug.LogWarning($"[SoundLibrary] '{clip.name}' SFX 클립이 ESfxClip enum 정의에 존재하지 않습니다.");
            }
        }
    }

    // 에셋 누락 검출을 수행하여 경고 메시지를 로그에 남긴다.
    private void ValidateClips()
    {
        int bgmEnumCount = Enum.GetValues(typeof(EBgmType)).Length;
        for (int i = 0; i < bgmEnumCount; i++)
        {
            if (_runtimeBgmClips[i] == null)
            {
                EBgmType type = (EBgmType)i;
                Debug.LogWarning($"[SoundLibrary] BGM '{type}' 클립이 할당되지 않았거나 파일명이 다릅니다.");
            }
        }

        int sfxEnumCount = Enum.GetValues(typeof(ESfxClip)).Length;
        for (int i = 0; i < sfxEnumCount; i++)
        {
            if (_runtimeSfxClips[i] == null)
            {
                ESfxClip clip = (ESfxClip)i;
                Debug.LogWarning($"[SoundLibrary] SFX '{clip}' 클립이 카테고리 리스트에 할당되지 않았거나 파일명이 다릅니다.");
            }
        }
    }
}
