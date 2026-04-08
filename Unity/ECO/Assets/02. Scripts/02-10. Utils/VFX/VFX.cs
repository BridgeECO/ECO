using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VFX : MonoBehaviour
{
    private List<ParticleSystem> _particleSystems;

    private void Awake()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>().ToList();
    }

    public void Play()
    {
        foreach (var particleSystem in _particleSystems)
        {
            particleSystem.Play();
        }
    }
}
