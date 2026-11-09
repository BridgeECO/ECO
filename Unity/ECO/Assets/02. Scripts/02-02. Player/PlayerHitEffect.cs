using UnityEngine;
using VInspector;

/// <summary>
/// 풀에서 꺼낼 때마다 파티클을 처음부터 재생하고, 재생이 끝나면 스스로 풀로 돌아간다.
/// 루트 파티클 시스템의 Stop Action이 Callback이어야 OnParticleSystemStopped가 불린다.
/// </summary>
// VFX가 먼저 붙어 있어야 VFX.Awake가 파티클 목록을 채운 뒤 이 OnEnable이 재생을 건다.
[RequireComponent(typeof(VFX))]
public class PlayerHitEffect : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private VFX _vfx;

    private void OnEnable()
    {
        _vfx.Play();
    }

    private void OnParticleSystemStopped()
    {
        PoolManager.Instance.Push(EPoolable.PlayerHitEffect, gameObject);
    }
}
