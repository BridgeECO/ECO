/// <summary>
/// InputHandler의 입력 차단을 한 벌 빌려 쓴다.
///
/// 자기가 건 차단만 해제하므로, 리스폰·씬 전환처럼 동시에 차단을 걸어 둔 다른 시스템의
/// 잠금까지 풀어 버리지 않는다. 정리 경로가 여럿인 쪽(예외·비활성화·강제 종료)에서
/// 몇 번을 불러도 안전하도록 두 메서드 모두 멱등하다.
/// </summary>
public class InputBlockLease
{
    private bool _isHeld;

    public bool IsHeld => _isHeld;

    public void Acquire()
    {
        if (_isHeld)
        {
            return;
        }

        _isHeld = true;
        InputHandler.BlockInput();
    }

    public void Release()
    {
        if (!_isHeld)
        {
            return;
        }

        _isHeld = false;
        InputHandler.UnblockInput();
    }
}
