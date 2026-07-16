using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergyCore : MonoBehaviour, IEnergyReceiver
{
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

    public Transform ActivationPosition => _activationPosition;
    public Transform DeactivationPosition => _deactivationPosition;

    private SpriteRenderer _spriteRenderer;
    private Color _activeColor = Color.yellow;
    private Color _deactiveColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _deactiveColor = _spriteRenderer.color;
    }

    public void SetEnergyActive(bool isActive)
    {
        _spriteRenderer.color = isActive ? _activeColor : _deactiveColor;
        for (int i = 0; i < _registeredTerrains.Count; i++)
        {
            if (_registeredTerrains[i] == null)
            {
                continue;
            }
            _registeredTerrains[i].SetEnergyActive(isActive);
        }

        PlayEnergySfx(isActive);
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
}
