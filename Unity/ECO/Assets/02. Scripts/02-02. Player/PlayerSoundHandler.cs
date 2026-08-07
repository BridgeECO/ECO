using UnityEngine;

/// <summary>
/// PC의 상태 전환과 지형 타입을 조합하여 올바른 SFX를 SoundManager로 전달하는 핸들러.
///   - PlayerStateMachine이 생성자에서 인스턴스를 소유하며, OnStateChanged 이벤트를 통해 상태 변화를 수신한다.
///   - 지형 타입 판별은 PlayerSensor가 제공하는 IsOnPlatform 플래그와
///     별도로 주입된 ETerrainSoundType(씬 단위 기본 지형)으로 결정한다.
///   - Hover Loop 사운드는 HoverState Enter/Exit 시점에 Play/Stop 된다.
///   - Walk Loop 사운드는 Grounded 상태의 UpdateWalkSound 호출로 매 프레임 갱신된다.
/// </summary>
public class PlayerSoundHandler
{
    private readonly PlayerSensor _sensor;
    // 씬 기본 지형 타입 — PlayerStateMachine이 씬 진입 시 Set으로 주입
    private ETerrainSoundType _defaultTerrainType = ETerrainSoundType.Scrap;
    private bool _isHoverLoopPlaying;
    // Hover 이탈(루프 정지) 감지를 위해 직전 상태를 기억한다.
    private EPlayerState _lastState = EPlayerState.Grounded;
    // Walk SFX 쿨다운 (Loop 의미이지만 PlayerSFX 풀 방식이라 주기 재생)
    private float _walkSfxTimer;
    private const float WALK_SFX_INTERVAL = 0.38f;

    public PlayerSoundHandler(PlayerSensor sensor)
    {
        _sensor = sensor;
    }

    // ──────────────────────────────────────────
    // 외부 설정
    // ──────────────────────────────────────────

    /// <summary>씬 전환 시 해당 씬의 기본 지형 타입을 설정한다.</summary>
    public void SetDefaultTerrainType(ETerrainSoundType type)
    {
        _defaultTerrainType = type;
    }

    // ──────────────────────────────────────────
    // 상태별 진입/종료 SFX (PlayerStateMachine.OnStateChanged 구독)
    // ──────────────────────────────────────────

    /// <summary>
    /// 상태 전이 시 사운드 매핑을 이 클래스가 소유한다.
    /// (FSM이 상태별 사운드 메서드를 switch로 직접 매핑하지 않도록 분리)
    /// </summary>
    public void HandleStateChanged(EPlayerState newState)
    {
        if (_lastState == EPlayerState.Hover && newState != EPlayerState.Hover)
        {
            StopHoverLoop();
        }
        _lastState = newState;

        switch (newState)
        {
            case EPlayerState.Grounded:
                PlayLandSfx();
                break;
            case EPlayerState.Airborne:
                StopWalkLoop();
                break;
            case EPlayerState.WallSlide:
                StopWalkLoop();
                PlayHitWallSfx();
                break;
            case EPlayerState.Hover:
                StopWalkLoop();
                PlayHoverLoop();
                break;
            case EPlayerState.Dash:
                StopWalkLoop();
                PlayDashSfx();
                break;
        }
    }

    // ──────────────────────────────────────────
    // 매 프레임 호출 (PlayerStateMachine.Update → Grounded 상태일 때만)
    // ──────────────────────────────────────────

    /// <summary>
    /// Grounded 상태 Update에서 호출하여 이동 중에만 Walk SFX를 주기적으로 재생한다.
    /// deltaTime을 인자로 받아 Time 의존성을 내부에서 제거하지 않는다.
    /// </summary>
    public void UpdateWalkSound(float deltaTime, bool isMoving)
    {
        if (!isMoving)
        {
            StopWalkLoop();
            return;
        }

        _walkSfxTimer -= deltaTime;
        if (_walkSfxTimer > 0f)
        {
            return;
        }

        _walkSfxTimer = WALK_SFX_INTERVAL;
        PlayWalkSfx();
    }

    // ──────────────────────────────────────────
    // 피해 / 사망 SFX (PlayerLife에서 직접 호출)
    // ──────────────────────────────────────────

    public void PlayDamageSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        SoundManager.Instance.PlayPlayerSfx(ESfxClip.SE_PC_Damage);
    }

    public void PlayDeathSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        SoundManager.Instance.PlayPlayerSfx(ESfxClip.SE_PC_Death);
    }

    // ──────────────────────────────────────────
    // 내부 SFX 재생 메서드
    // ──────────────────────────────────────────

    private void PlayWalkSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        ESfxClip clip = PlayerTerrainSfxResolver.ResolveWalkClip(_sensor, _defaultTerrainType);
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayLandSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        ESfxClip clip = PlayerTerrainSfxResolver.ResolveLandClip(_sensor, _defaultTerrainType);
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayHitWallSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        ESfxClip clip = PlayerTerrainSfxResolver.ResolveHitWallClip(_sensor, _defaultTerrainType);
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayDashSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        ESfxClip clip = PlayerTerrainSfxResolver.ResolveDashClip(_sensor, _defaultTerrainType);
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayHoverLoop()
    {
        if (!SoundManager.HasInstance || _isHoverLoopPlaying)
        {
            return;
        }
        _isHoverLoopPlaying = true;
        SoundManager.Instance.PlayPlayerLoopSfx(ESfxClip.SE_PC_Hovering);
    }

    private void StopHoverLoop()
    {
        if (!SoundManager.HasInstance || !_isHoverLoopPlaying)
        {
            return;
        }
        _isHoverLoopPlaying = false;
        SoundManager.Instance.StopPlayerLoopSfx(ESfxClip.SE_PC_Hovering);
    }

    private void StopWalkLoop()
    {
        _walkSfxTimer = 0f;
        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.StopPlayerSfx(ESfxClip.SE_PC_WalkingScrap);
            SoundManager.Instance.StopPlayerSfx(ESfxClip.SE_PC_WalkingMold);
            SoundManager.Instance.StopPlayerSfx(ESfxClip.SE_PC_WalkingPlatform);
        }
    }
}
