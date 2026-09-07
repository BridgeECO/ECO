using System;
using UnityEngine;

/// <summary>ESC 스킵과 안내 표시 요청을 감시한다. 세션이 재생 중 매 프레임 Tick한다.</summary>
public class CutsceneSkipWatcher
{
    private const float DEFAULT_ARM_DELAY = 0.2f;

    public Action OnSkipRequested;
    public Action OnHintRequested;

    private bool _isArmed;
    private bool _isSkippable;
    private bool _isSuspended;
    private bool _isHintRequested;
    private float _armDelayRemaining;

    public bool IsArmed => _isArmed;

    public void Arm(bool isSkippable, float armDelay = DEFAULT_ARM_DELAY)
    {
        _isArmed = true;
        _isSkippable = isSkippable;
        _isSuspended = false;
        _isHintRequested = false;
        _armDelayRemaining = Mathf.Max(0f, armDelay);
    }

    public void Disarm()
    {
        _isArmed = false;
        _isSuspended = false;
        _armDelayRemaining = 0f;
    }

    /// <summary>끊으면 망가지는 스텝이 도는 동안 감시를 멈춘다.</summary>
    public void SetSuspended(bool isSuspended)
    {
        _isSuspended = isSuspended;
    }

    public void Tick(float deltaTime)
    {
        if (!_isArmed || _isSuspended)
        {
            return;
        }

        // 컷씬을 발동시킨 그 입력이 같은 프레임에 곧바로 안내를 띄우는 것을 막는다.
        if (0f < _armDelayRemaining)
        {
            _armDelayRemaining = Mathf.Max(0f, _armDelayRemaining - deltaTime);
            return;
        }

        // ESC 판정을 스킵 가능 여부 바깥에 둔다. 안쪽에 두면 스킵 불가 컷씬에서 ESC가
        // anyKeyDown 분기로 새어 "ESC로 건너뛰기" 안내가 뜬다. 플레이어에게 거짓말이 된다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isSkippable)
            {
                OnSkipRequested?.Invoke();
            }

            return;
        }

        // anyKeyDown은 bool 조회라 inputString과 달리 GC.Alloc이 없다.
        if (_isHintRequested || !_isSkippable || !Input.anyKeyDown)
        {
            return;
        }

        _isHintRequested = true;
        OnHintRequested?.Invoke();
    }
}
