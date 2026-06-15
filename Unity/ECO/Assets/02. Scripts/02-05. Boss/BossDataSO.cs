using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "BossData", menuName = "Scriptable Objects/BossDataSO")]
public class BossDataSO : ScriptableObject
{
    [Foldout("Stats")]
    [SerializeField]
    [Min(0f)]
    private float _baseSpeed;

    [SerializeField]
    [Min(0f)]
    private float _catchUpSpeed;

    [Tooltip("이 거리 이상 플레이어와 멀어지면 catchUpSpeed로 가속합니다.")]
    [SerializeField]
    private float _catchUpDistanceThreshold;

    [SerializeField]
    [Min(0f)]
    private float _jumpSpeed;

    public float BaseSpeed => _baseSpeed;
    public float CatchUpSpeed => _catchUpSpeed;
    public float CatchUpDistanceThreshold => _catchUpDistanceThreshold;
    public float JumpSpeed => _jumpSpeed;
}
