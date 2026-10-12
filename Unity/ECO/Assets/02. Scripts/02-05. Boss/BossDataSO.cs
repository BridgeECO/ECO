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

    [SerializeField]
    [Min(0f)]
    [Tooltip("추격 속도로 전환되는 플레이어와의 거리입니다.")]
    private float _catchUpStartDistance;

    [SerializeField]
    [Min(0f)]
    [Tooltip("기본 속도로 돌아오는 플레이어와의 거리입니다.")]
    private float _catchUpEndDistance;

    [SerializeField]
    [Min(0f)]
    private float _jumpSpeed;

    [Foldout("Sfx")]
    [SerializeField]
    private ESfxClip _ShoutSfx;

    public float BaseSpeed => _baseSpeed;
    public float CatchUpSpeed => _catchUpSpeed;
    public float CatchUpStartDistance => _catchUpStartDistance;
    public float CatchUpEndDistance => _catchUpEndDistance;
    public float JumpSpeed => _jumpSpeed;
    public ESfxClip ShoutSfx => _ShoutSfx;
}
