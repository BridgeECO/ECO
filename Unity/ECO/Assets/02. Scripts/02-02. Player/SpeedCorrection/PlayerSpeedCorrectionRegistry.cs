using UnityEngine;

/// <summary>
/// 현재 걸려 있는 추가 속도 보정들을 보관하고 합산한다.
/// 매 프레임 합산되므로 할당이 생기지 않도록 고정 배열에 구조체를 제자리 갱신한다.
/// </summary>
public class PlayerSpeedCorrectionRegistry
{
    private const int MAX_CORRECTION_SOURCES = 4;

    private PlayerSpeedCorrection[] _corrections = new PlayerSpeedCorrection[MAX_CORRECTION_SOURCES];
    private int _correctionCount;
    private bool _hasLoggedOverflow;

    public int CorrectionCount => _correctionCount;

    // 같은 소스가 여러 콜라이더로 중복 등록해도 항목이 하나만 남도록 덮어쓴다.
    public void SetCorrection(int sourceId, float magnitude, ESpeedCorrectionDirection direction)
    {
        int index = IndexOf(sourceId);
        if (index < 0)
        {
            if (MAX_CORRECTION_SOURCES <= _correctionCount)
            {
                LogOverflowOnce();
                return;
            }

            index = _correctionCount;
            _correctionCount++;
        }

        _corrections[index].SourceId = sourceId;
        _corrections[index].Magnitude = magnitude;
        _corrections[index].Direction = direction;
    }

    public void RemoveCorrection(int sourceId)
    {
        int index = IndexOf(sourceId);
        if (index < 0)
        {
            return;
        }

        // 합산은 순서에 영향을 받지 않으므로 마지막 항목을 당겨와 O(1)로 지운다.
        _correctionCount--;
        _corrections[index] = _corrections[_correctionCount];
    }

    public void ClearCorrections()
    {
        _correctionCount = 0;
        _hasLoggedOverflow = false;
    }

    public float ResolveSum(float horizontalInput)
    {
        float sum = 0f;
        for (int i = 0; i < _correctionCount; i++)
        {
            sum += ResolveValue(_corrections[i], horizontalInput);
        }
        return sum;
    }

    // 좌측 보정은 왼쪽 이동을 세기만큼 빠르게, 오른쪽 이동을 세기만큼 느리게 만든다.
    // 부호를 붙여 풀면 두 경우 모두 속도에 -세기를 더한 것과 같아, 방향 3종이 가산항 하나로 환원된다.
    private float ResolveValue(PlayerSpeedCorrection correction, float horizontalInput)
    {
        switch (correction.Direction)
        {
            case ESpeedCorrectionDirection.Left:
                return -correction.Magnitude;
            case ESpeedCorrectionDirection.Right:
                return correction.Magnitude;
            default:
                // 양방향은 이동 방향 쪽으로 세기를 더한다. Mathf.Sign(0f)이 1f라 가만히 선 플레이어를
                // 밀어버리므로 부호 함수 대신 입력을 그대로 곱한다.
                // PlayerInput.HorizontalInput이 GetAxisRaw라 -1/0/1로 양자화된 덕분에 성립한다.
                return horizontalInput * correction.Magnitude;
        }
    }

    private int IndexOf(int sourceId)
    {
        for (int i = 0; i < _correctionCount; i++)
        {
            if (_corrections[i].SourceId == sourceId)
            {
                return i;
            }
        }
        return -1;
    }

    // 매 물리 스텝 호출될 수 있는 경로라 보간 문자열 할당이 생기지 않도록 한 번만 남긴다.
    private void LogOverflowOnce()
    {
        if (_hasLoggedOverflow)
        {
            return;
        }
        _hasLoggedOverflow = true;
        Debug.LogWarning("[PlayerSpeedCorrectionRegistry] 동시에 걸 수 있는 추가 속도 보정 수를 초과했습니다.");
    }
}
