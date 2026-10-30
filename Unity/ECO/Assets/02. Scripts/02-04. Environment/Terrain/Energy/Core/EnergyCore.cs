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

    [Tooltip("이 코어의 지형만 안착시키는 스냅포인트입니다. 이동형 코어에서만 동작하며, 지형의 웨이포인트 경로 위에 둡니다.")]
    [SerializeField]
    private List<SnapPoint> _snapPoints = new List<SnapPoint>();

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
    private CoreSnapCoordinator _snapCoordinator;

    public Transform ActivationPosition => _activationPosition;
    public Transform DeactivationPosition => _deactivationPosition;
    public IReadOnlyList<TerrainObject> RegisteredTerrains => _registeredTerrains;
    public IReadOnlyList<SnapPoint> SnapPoints => _snapPoints;
    public bool IsActive => _isActive;

    // 스냅포인트가 있는 이동형 코어에만 있다. 없으면 null이다.
    public CoreSnapCoordinator SnapCoordinator => _snapCoordinator;

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
        InitSnapCoordinator();
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
        SnapRangeGizmoDrawer.Draw(_registeredTerrains, _snapPoints);
#endif
    }

    public void SetEnergyActive(bool isActive)
    {
        bool isChanged = _isActive != isActive;
        _isActive = isActive;
        _spriteRenderer.color = isActive ? _activeColor : _deactiveColor;

        // 지형에 전달하기 전에 움직이던 지형을 기억해 두어야, 이번 신호로 멈춘 지형만 스냅포인트에 안착시킬 수 있다.
        if (isChanged)
        {
            _snapCoordinator?.CaptureMovingTerrains();
        }

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

        _snapCoordinator?.SettleStoppedTerrains();
        PlayEnergySfx(isActive);
        OnActiveChanged?.Invoke(isActive);
    }

    // 스냅포인트는 위치를 옮기는 지형에만 의미가 있다. 스냅포인트가 없는 코어는 예전과 똑같이 동작한다.
    private void InitSnapCoordinator()
    {
        if (_snapPoints.Count == 0 || !IsMovableCore)
        {
            return;
        }
        _snapCoordinator = new CoreSnapCoordinator(_registeredTerrains, _snapPoints);
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
