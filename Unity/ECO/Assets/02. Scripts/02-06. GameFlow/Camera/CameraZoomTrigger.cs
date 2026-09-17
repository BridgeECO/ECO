using UnityEngine;
using VInspector;

[RequireComponent(typeof(Collider2D))]
public class CameraZoomTrigger : MonoBehaviour
{
    private const float MIN_LINE_LENGTH = 0.001f;

    [Foldout("Points")]
    [Tooltip("줌 변화가 시작되는 월드 위치입니다.")]
    [SerializeField]
    private Transform _startPoint;

    [Tooltip("줌 변화가 완료되는 월드 위치입니다.")]
    [SerializeField]
    private Transform _endPoint;

    [Foldout("Camera Settings")]
    [Tooltip("시작 지점에서 카메라 컨트롤러가 사용할 Z 좌표입니다.")]
    [SerializeField]
    private float _startCameraZ;

    [Tooltip("끝 지점에서 카메라 컨트롤러가 사용할 Z 좌표입니다. 더 작은 값은 줌아웃, 더 큰 값은 줌인입니다.")]
    [SerializeField]
    private float _targetCameraZ;

    [Tooltip("진행도에 따른 줌 변화 곡선입니다.")]
    [SerializeField]
    private AnimationCurve _zoomCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Foldout("Gizmo Visual Settings")]
    [SerializeField]
    private Vector2 _previewAspectRatio = new Vector2(16f, 9f);

    [Range(1f, 179f)]
    [SerializeField]
    private float _previewFieldOfView = 60f;

    private CameraController _cameraController;
    private Transform _playerTransform;
    private int _playerColliderCount;
    private float _originalCameraZ;

    private void Awake()
    {
        TrySetCameraController();
    }

    private void Update()
    {
        if (_playerColliderCount == 0 ||
            _playerTransform == null ||
            _startPoint == null ||
            _endPoint == null ||
            !TrySetCameraController())
        {
            return;
        }

        UpdateCameraZoom();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.Player)))
        {
            return;
        }

        Rigidbody2D playerBody = other.attachedRigidbody;
        if (playerBody == null)
        {
            return;
        }
        if (!TrySetCameraController())
        {
            return;
        }
        _cameraController.SetBottomAnchored(true);
        if (_playerColliderCount == 0)
        {
            _playerTransform = playerBody.transform;
            _originalCameraZ = _cameraController.transform.position.z;
        }

        _playerColliderCount += 1;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.Player)) || _playerColliderCount == 0)
        {
            return;
        }

        _playerColliderCount -= 1;
        if (_playerColliderCount != 0)
        {
            return;
        }
        _playerTransform = null;
    }

    private void OnDrawGizmos()
    {
        DrawGizmosVisuals();
    }

    private bool TrySetCameraController()
    {
        if (_cameraController != null)
        {
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        _cameraController = mainCamera.GetComponentInParent<CameraController>();
        return _cameraController != null;
    }

    private void UpdateCameraZoom()
    {
        Vector2 startPosition = _startPoint.position;
        Vector2 endPosition = _endPoint.position;
        Vector2 lineDirection = endPosition - startPosition;
        float lineLength = lineDirection.magnitude;
        if (lineLength <= MIN_LINE_LENGTH)
        {
            return;
        }

        Vector2 playerOffset = (Vector2)_playerTransform.position - startPosition;
        float rawProgress = Vector2.Dot(playerOffset, lineDirection / lineLength) / lineLength;
        float progress = Mathf.Clamp01(rawProgress);
        float evaluatedProgress = _zoomCurve.Evaluate(progress);
        float cameraZ = Mathf.Lerp(_startCameraZ, _targetCameraZ, evaluatedProgress);
        SetCameraZ(cameraZ);
    }

    private void SetCameraZ(float cameraZ)
    {
        Vector3 cameraPosition = _cameraController.transform.position;
        cameraPosition.z = cameraZ;
        _cameraController.transform.position = cameraPosition;
    }

    private void DrawGizmosVisuals()
    {
        if (_startPoint == null || _endPoint == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_startPoint.position, 0.4f);
        DrawCameraView(_startPoint.position, _startCameraZ, Color.cyan);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_endPoint.position, 0.4f);
        DrawCameraView(_endPoint.position, _targetCameraZ, Color.magenta);
    }

    private void DrawCameraView(Vector3 centerPosition, float cameraZ, Color color)
    {
        Camera mainCamera = Camera.main;
        float aspect = mainCamera != null
            ? mainCamera.aspect
            : _previewAspectRatio.x / _previewAspectRatio.y;
        float fieldOfView = mainCamera != null ? mainCamera.fieldOfView : _previewFieldOfView;
        float halfHeight = Mathf.Abs(cameraZ) * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
        float height = halfHeight * 2f;

        Gizmos.color = color;
        Gizmos.DrawWireCube(centerPosition, new Vector3(height * aspect, height, 0f));
    }
}
