/// <summary>
/// 계기판에서 코어 하나와 그 코어침을 잇는다. 코어와 스냅 안착 상태를 읽어 코어침에 보여 줄 값을 정한다.
/// 상태형 코어침은 코어가 켜지면 가장자리까지 한 번에 빛난다.
/// 이동형 코어침은 지형이 움직이는 동안 가운데가 밝고, 스냅포인트에 가까울수록 빛이 가장자리로 차오르며, 실제로 안착하면 테두리가 켜진다.
/// </summary>
public class DashboardCoreBinding
{
    private EnergyCore _core;
    private UI_CoreNeedle _needle;
    private CoreSnapCoordinator _snapCoordinator;
    private bool _isMovableCore;
    private bool _wasMoving;

    public DashboardCoreBinding(EnergyCore core, UI_CoreNeedle needle)
    {
        _core = core;
        _needle = needle;
        _snapCoordinator = core.SnapCoordinator;
        _isMovableCore = core.IsMovableCore;
    }

    public void Subscribe()
    {
        _core.OnActiveChanged += HandleActiveChanged;
    }

    public void Unsubscribe()
    {
        _core.OnActiveChanged -= HandleActiveChanged;
    }

    public void Refresh()
    {
        if (!_isMovableCore)
        {
            _needle.SetGlowAmount(1f);
            _needle.SetLit(_core.IsActive);
            return;
        }

        bool hasSnapPoints = !ReferenceEquals(_snapCoordinator, null);
        bool isSettling = hasSnapPoints && _snapCoordinator.IsSettling;
        bool isDocked = hasSnapPoints && _snapCoordinator.IsDocked;
        _needle.SetGlowAmount(hasSnapPoints ? _snapCoordinator.GetProximity() : 0f);
        _needle.SetLit(_core.IsActive || isSettling || isDocked);
        _needle.SetDocked(isDocked);
    }

    // 이동형 코어의 지형은 코어가 켜져 있거나 안착하는 동안 움직이므로, 그동안과 멈춘 직후 한 번만 다시 그린다.
    public void RefreshIfMoving()
    {
        if (!_isMovableCore)
        {
            return;
        }

        bool isMoving = _core.IsActive || (!ReferenceEquals(_snapCoordinator, null) && _snapCoordinator.IsSettling);
        if (isMoving || _wasMoving)
        {
            Refresh();
        }
        _wasMoving = isMoving;
    }

    private void HandleActiveChanged(bool isActive)
    {
        Refresh();
    }
}
