using UnityEngine;

/// <summary>
/// 카메라 계열 컴포넌트를 런타임에 찾는다.
///
/// 카메라는 PersistentScene에, 컷씬은 리전 씬에 있어 인스펙터로 묶을 수 없다.
/// BossCinematicBase가 쓰는 Camera.main 경로를 따르되, Start가 아니라 재생 시작 시점에 푼다.
/// 카메라 프리팹의 사본으로 만들어진 씬이 있어 구성이 다를 수 있으므로
/// CameraController만 필수로 보고 나머지는 없어도 진행한다.
/// </summary>
public static class CutsceneCameraResolver
{
    public static bool TryResolve(out CameraController controller, out CameraEffect cameraEffect,
        out CameraRoomTransition roomTransition)
    {
        controller = null;
        cameraEffect = null;
        roomTransition = null;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        // 움직이는 것은 루트고 렌더링과 흔들기는 자식이다. 두 단을 따로 집는다.
        controller = mainCamera.GetComponentInParent<CameraController>();
        cameraEffect = mainCamera.GetComponent<CameraEffect>();

        if (controller != null)
        {
            roomTransition = controller.GetComponent<CameraRoomTransition>();
        }

        return controller != null;
    }
}
