using UnityEngine;
using VInspector;

public class CameraController : MonoBehaviour
{
    [Header("Camera Offset")]
    [SerializeField]
    private float _cameraYOffset;

    private Vector2 _currentRoomMin;
    private Vector2 _currentRoomMax;
    private Transform _playerTransform;

    private float _halfCamHeight;
    private float _halfCamWidth;

    public bool IsFollowingPlayer { get; set; } = true;

    private void Awake()
    {
        InitCameraController();
        Vector3 clamped = GetClampedPosition();
        transform.position = clamped;
    }

    private void LateUpdate()
    {
        if (!IsFollowingPlayer)
        {
            return;
        }
        FollowPlayer();
    }

    private void InitCameraController()
    {
        Camera camera = Camera.main;
        _halfCamHeight = camera.orthographicSize;
        _halfCamWidth = _halfCamHeight * camera.aspect;
        _playerTransform = Object.FindAnyObjectByType<PlayerStateMachine>(FindObjectsInactive.Include).transform;
        _playerTransform.gameObject.SetActive(true);
        _playerTransform.position = Vector3.zero;
    }

    private void FollowPlayer()
    {
        transform.position = GetClampedPosition();
    }

    public void SetRoomBounds(Vector2 roomMin, Vector2 roomMax)
    {
        _currentRoomMin = roomMin;
        _currentRoomMax = roomMax;
    }

    public Vector3 GetClampedPosition()
    {
        float clampedX = ClampAxis(_playerTransform.position.x, _currentRoomMin.x, _currentRoomMax.x, _halfCamWidth);
        float clampedY = ClampAxis(_playerTransform.position.y + _cameraYOffset, _currentRoomMin.y, _currentRoomMax.y, _halfCamHeight);
        return new Vector3(clampedX, clampedY, transform.position.z);
    }

    private float ClampAxis(float target, float roomMin, float roomMax, float halfCamSize)
    {
        float clampMin = roomMin + halfCamSize;
        float clampMax = roomMax - halfCamSize;
        return (clampMax < clampMin) ? (roomMin + roomMax) * 0.5f : Mathf.Clamp(target, clampMin, clampMax);
    }
}