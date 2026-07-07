using UnityEngine;

// 에너지 구 추적 중 카메라 이동을 담당하는 순수 C# 로직 클래스.
// MonoBehaviour 없이 Transform과 CameraController 참조만으로 동작하며,
// EnergyLineTracker(MB)가 LateUpdate에서 UpdatePosition을 호출하여 구동한다.
public class EnergyLineTrackingCameraLogic
{
    private EnergyLine _targetLine;
    private CameraController _cameraController;
    private Vector3 _velocity = Vector3.zero;

    public bool IsTracking { get; private set; }

    // 트래킹 시작: CameraController의 플레이어 추적을 중단하고 상태를 초기화한다.
    public void StartTracking(EnergyLine targetLine, CameraController cameraController)
    {
        _targetLine = targetLine;
        _cameraController = cameraController;
        _velocity = Vector3.zero;
        IsTracking = true;
        _cameraController.IsFollowingPlayer = false;
    }

    // 트래킹 종료: 카메라를 플레이어 기준 클램프 위치로 즉시 스냅하고 추적을 재개한다.
    // 페이드 아웃 직후 호출되므로 위치 점프가 화면에 드러나지 않는다.
    public void StopTracking(Transform cameraTransform)
    {
        IsTracking = false;

        Vector3 returnPos = _cameraController.GetClampedPosition();
        returnPos.z = cameraTransform.position.z;
        cameraTransform.position = returnPos;

        _cameraController.IsFollowingPlayer = true;
        _targetLine = null;
        _cameraController = null;
    }

    // EnergyLineTracker의 LateUpdate에서 매 프레임 호출된다.
    // 룸 클램핑 없이 에너지 구 Head 위치를 SmoothDamp로 추적하여
    // 라인 끝이 룸 바운드 밖에 있어도 자유롭게 따라간다.
    public void UpdatePosition(Transform cameraTransform, float smoothTime)
    {
        if (!IsTracking || _targetLine == null)
        {
            return;
        }

        if (!_targetLine.TryGetSegmentHeadWorldPosition(out Vector3 headWorldPos))
        {
            return;
        }

        Vector3 targetPos = new Vector3(headWorldPos.x, headWorldPos.y, cameraTransform.position.z);
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref _velocity, smoothTime);
    }
}
