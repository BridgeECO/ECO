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
    private float _jumpSpeed;

    [Foldout("Sfx")]
    [SerializeField]
    private ESfxClip _ShoutSfx;

    public float BaseSpeed => _baseSpeed;
    public float CatchUpSpeed => _catchUpSpeed;
    public float JumpSpeed => _jumpSpeed;
    public ESfxClip ShoutSfx => _ShoutSfx;
}
