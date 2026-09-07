using UnityEngine;

/// <summary>
/// 스텝들이 공유하는 참조와 소유권 묶음. 컷씬 한 편의 수명을 가진다.
///
/// 전역 플레이어 접근점이 없고 Find 계열이 금지되어 있어, 플레이어는 트리거가 받은
/// 콜라이더나 NPC가 쥔 PlayerInput에서 역수집한다. (TutorialConditionContext와 같은 제약)
///
/// 리스를 세션이 아니라 여기에 두는 이유: 조작 반환 스텝처럼 재생 도중 소유권을 넘기는
/// 스텝이 있어야 하는데, 스텝이 손에 쥘 수 있는 것은 컨텍스트뿐이다.
/// </summary>
public class CutsceneContext
{
    public Transform Owner { get; private set; }
    public PlayerStateMachine Player { get; private set; }
    public CameraController CameraController { get; private set; }
    public CameraEffect CameraEffect { get; private set; }
    public CameraRoomTransition RoomTransition { get; private set; }

    public InputBlockLease InputLease { get; } = new InputBlockLease();
    public CutsceneActorFreeze ActorFreeze { get; } = new CutsceneActorFreeze();
    public CutsceneCameraLease CameraLease { get; } = new CutsceneCameraLease();
    public CutsceneCameraMover CameraMover { get; } = new CutsceneCameraMover();

    public bool HasPlayer => Player != null;

    public CutsceneContext(Transform owner)
    {
        Owner = owner;
    }

    public void BindPlayer(PlayerStateMachine player)
    {
        if (player != null)
        {
            Player = player;
        }
    }

    /// <summary>트리거에 들어온 콜라이더에서 부모를 거슬러 플레이어를 찾는다.</summary>
    public void BindPlayerFrom(Collider2D playerCollider)
    {
        if (playerCollider == null)
        {
            return;
        }

        BindPlayer(playerCollider.GetComponentInParent<PlayerStateMachine>());
    }

    /// <summary>
    /// 카메라 참조를 푼다. 씬이 언로드되면 붙들고 있던 참조가 가짜 null이 되므로
    /// 재생을 시작할 때마다 다시 확인한다.
    /// </summary>
    public bool TryResolveCamera()
    {
        if (CameraController != null)
        {
            return true;
        }

        CutsceneCameraResolver.TryResolve(out CameraController controller, out CameraEffect cameraEffect,
            out CameraRoomTransition roomTransition);

        CameraController = controller;
        CameraEffect = cameraEffect;
        RoomTransition = roomTransition;

        return CameraController != null;
    }
}
