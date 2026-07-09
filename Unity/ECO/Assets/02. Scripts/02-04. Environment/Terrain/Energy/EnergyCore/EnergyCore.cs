using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergyCore : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<TerrainObject> _registeredTerrains = new List<TerrainObject>();

    /// <summary>
    /// 에너지 공급 중 Loop SFX를 재생할 전용 AudioSource.
    /// 코어가 활성화된 동안 계속 재생되며, 비활성화 시 자동 정지된다.
    /// </summary>
    [SerializeField]
    private AudioSource _feedingAudioSource;

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

        if (isActive)
        {
            SoundManager.Instance.PlayPlayerSfx(ESfxClip.SE_Energy_CoreOn);
            if (_feedingAudioSource != null)
            {
                _feedingAudioSource.Play();
            }
        }
        else
        {
            SoundManager.Instance.PlayPlayerSfx(ESfxClip.SE_Energy_CoreOff);
            if (_feedingAudioSource != null)
            {
                _feedingAudioSource.Stop();
            }
        }
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
