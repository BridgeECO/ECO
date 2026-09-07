using UnityEngine;
using VInspector;

public class PlayerCornerCorrector : MonoBehaviour
{
    [Foldout("Head Colliders")]
    [SerializeField]
    private Collider2D _headLeft;

    [SerializeField]
    private Collider2D _headCenter;

    [SerializeField]
    private Collider2D _headRight;

    [Foldout("Body Reference")]
    [SerializeField]
    private Collider2D _bodyCollider;

    [Foldout("Settings")]
    [SerializeField]
    private LayerMask _terrainLayer;

    [SerializeField]
    private float _correctionSpeed = 15f;

    private PlayerMotor _motor;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _motor = GetComponentInParent<PlayerMotor>();
        _rigidbody = GetComponentInParent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        UpdateCornerCorrection();
    }

    private void UpdateCornerCorrection()
    {
        // rigidbody.position을 직접 밀어내므로 linearVelocity를 0으로 눌러도 막히지 않는다.
        // 컷씬은 천장이 낮은 자리에서 플레이어를 세우는 일이 잦아 이 경로가 상시 활성이다.
        if (_motor.IsFrozen)
        {
            return;
        }

        if (_motor.Velocity.y <= 0f)
        {
            return;
        }

        bool isCenterHitting = _headCenter.IsTouchingLayers(_terrainLayer);
        if (isCenterHitting)
        {
            _motor.SetVelocityY(0f);
            return;
        }

        bool isLeftHitting = _headLeft.IsTouchingLayers(_terrainLayer);
        bool isRightHitting = _headRight.IsTouchingLayers(_terrainLayer);
        if (isLeftHitting && !isRightHitting)
        {
            ApplyCorrection(1f);
        }
        if (isRightHitting && !isLeftHitting)
        {
            ApplyCorrection(-1f);
        }
    }

    private void ApplyCorrection(float direction)
    {
        float pushAmount = _bodyCollider.bounds.extents.x * _correctionSpeed * Time.fixedDeltaTime;
        _rigidbody.position += Vector2.right * (direction * pushAmount);
    }
}
