using System;

/// <summary>
/// 컷씬이 카메라 제어권을 빌린다. 자기가 마지막으로 쓴 값이 그대로 남아 있을 때만 되돌리고,
/// 그사이 다른 주인이 값을 바꿔 놨으면 손대지 않는다.
/// </summary>
public class CutsceneCameraLease
{
    public Action OnContested;

    private CameraController _controller;
    private CameraRoomTransition _roomTransition;

    private bool _isHeld;
    private bool _wasFollowingPlayer;
    private bool _lastWrittenFollowing;

    public bool IsHeld => _isHeld;

    public void Acquire(CutsceneContext context)
    {
        if (_isHeld || !context.TryResolveCamera())
        {
            return;
        }

        _controller = context.CameraController;
        _roomTransition = context.RoomTransition;
        _wasFollowingPlayer = _controller.IsFollowingPlayer;

        _isHeld = true;
        WriteFollowing(false);

        // 방 경계가 바뀌면 클램프 결과가 함께 바뀌어, 진행 중인 패닝의 목표가 낡은 값이 된다.
        // Region이 전환 연출을 건너뛰는 경로로 오면 RoomChanged가 아예 발행되지 않으므로
        // 이벤트가 아니라 바운드 변경 자체를 감시한다. 카메라 사본으로 만들어진 씬에는
        // CameraRoomTransition이 없을 수 있어 그쪽은 있을 때만 구독한다.
        _controller.OnRoomBoundsChanged += HandleContested;
        if (_roomTransition != null)
        {
            _roomTransition.OnRoomTransitionStarted += HandleContested;
        }
    }

    /// <summary>
    /// 스텝마다 부른다. 카메라를 되찾아가는 경로가 셋이라(패닝 종료, 방 전환 완료, 보스 컷씬 finally)
    /// bool 비교 하나로 전부 덮는다.
    /// </summary>
    public void Reassert()
    {
        if (!_isHeld || _controller == null || !_controller.IsFollowingPlayer)
        {
            return;
        }

        WriteFollowing(false);
    }

    public void Release()
    {
        if (!_isHeld)
        {
            return;
        }

        _isHeld = false;

        if (_controller != null)
        {
            _controller.OnRoomBoundsChanged -= HandleContested;

            // 남이 이미 바꿔 놨으면 그 주인의 결정을 존중해 손대지 않는다.
            if (_controller.IsFollowingPlayer == _lastWrittenFollowing)
            {
                _controller.IsFollowingPlayer = _wasFollowingPlayer;
            }
        }

        if (_roomTransition != null)
        {
            _roomTransition.OnRoomTransitionStarted -= HandleContested;
        }

        _controller = null;
        _roomTransition = null;
    }

    private void WriteFollowing(bool isFollowing)
    {
        _controller.IsFollowingPlayer = isFollowing;
        _lastWrittenFollowing = isFollowing;
    }

    private void HandleContested()
    {
        if (!_isHeld)
        {
            return;
        }

        OnContested?.Invoke();
    }
}
