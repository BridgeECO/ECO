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

    [Tooltip("_catchUpSpeed로 변경이 시작되는 거리")]
    [SerializeField]
    private float _catchUpStartDistance;

    [Tooltip("_catchUpSpeed이 끝나는 거리")]
    [SerializeField]
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
