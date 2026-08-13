using System;
using UnityEngine;

[Serializable]
public sealed class BossPathSection
{
    [SerializeField]
    [Tooltip("이 구간에서 보스가 향할 도착점입니다.")]
    private Transform _endPoint;

    [SerializeField]
    [Min(0.01f)]
    [Tooltip("기본 이동 속도에 곱할 배속입니다.")]
    private float _speedMultiplier = 1f;

    public Transform EndPoint => _endPoint;
    public float SpeedMultiplier => 0f < _speedMultiplier ? _speedMultiplier : 1f;

    public BossPathSection(Transform endPoint, float speedMultiplier)
    {
        _endPoint = endPoint;
        _speedMultiplier = speedMultiplier;
    }
}
