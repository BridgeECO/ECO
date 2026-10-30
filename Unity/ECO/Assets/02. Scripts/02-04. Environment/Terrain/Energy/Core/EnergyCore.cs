using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergyCore : MonoBehaviour, IEnergyReceiver
{
    public Action<bool> OnActiveChanged;

    [Foldout("Hierarchy")]
    [SerializeField]
    private List<TerrainObject> _registeredTerrains = new List<TerrainObject>();

    /// <summary>
    /// CoreOn / CoreOff 일회성 SFX 전용 AudioSource.
    /// 이 오브젝트 위에서 직접 재생하여 위치 기반 방향·감쇠를 자연스럽게 처리한다.
    /// </summary>
    [SerializeField]
    private AudioSource _sfxAudioSource;

    [Foldout("Energy")]
    [SerializeField]
    private Transform _activationPosition;

    [SerializeField]
    private Transform _deactivationPosition;

    private SpriteRenderer _spriteRenderer;
    private Color _activeColor = Color.yellow;
    private Color _deactiveColor;
    private bool _isActive;

    public Transform ActivationPosition => _activationPosition;
    public Transform DeactivationPosition => _deactivationPosition;
    public IReadOnlyList<TerrainObject> RegisteredTerrains => _registeredTerrains;
    public bool IsActive => _isActive;

    // 기획상 코어 종류는 등록된 지형의 기믹이 정한다. 위치를 옮기는 기믹이 있으면 이동형이다.
    public bool IsMovableCore
    {
        get
        {
            for (int i = 0; i < _registeredTerrains.Count; i++)
            {
                if (_registeredTerrains[i] != null && _registeredTerrains[i].HasMovementGimmick)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // 출구 지점을 따로 두지 않은 코어에서, 에너지가 완전히 빠져나갔다고 볼 거리. 그래프를 계산할 때 한 번만 읽는다.
    public float ExitRadius
    {
        get
        {
            Vector3 extents = GetComponent<SpriteRenderer>().bounds.extents;
            return Mathf.Max(extents.x, extents.y);
        }
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _deactiveColor = _spriteRenderer.color;
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.yellow;
        foreach (var terrain in _registeredTerrains)
        {
            if (terrain == null)
            {
                continue;
            }
            Vector3 terrainPos = terrain.transform.position;
            Gizmos.DrawLine(transform.position, terrainPos);
            Gizmos.DrawSphere(terrainPos, 0.2f);
        }
#endif
    }

    public void SetEnergyActive(bool isActive)
    {
        bool isChanged = _isActive != isActive;
        _isActive = isActive;
        _spriteRenderer.color = isActive ? _activeColor : _deactiveColor;

        // 리스폰 리셋으로 지형만 먼저 꺼진 경우에도 코어 상태에 다시 맞추도록 매번 전달한다.
        for (int i = 0; i < _registeredTerrains.Count; i++)
        {
            if (_registeredTerrains[i] == null)
            {
                continue;
            }
            _registeredTerrains[i].SetEnergyActive(isActive);
        }

        if (!isChanged)
        {
            return;
        }

        PlayEnergySfx(isActive);
        OnActiveChanged?.Invoke(isActive);
    }

    private void PlayEnergySfx(bool isActive)
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }

        ESfxClip clip = isActive ? ESfxClip.SE_Energy_CoreOn : ESfxClip.SE_Energy_CoreOff;
        SoundManager.Instance.PlaySfxOnSource(clip, _sfxAudioSource);
    }
}
