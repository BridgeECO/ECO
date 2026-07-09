using UnityEngine;

/// <summary>
/// PC의 상태 전환과 지형 타입을 조합하여 올바른 SFX를 SoundManager로 전달하는 핸들러.
///
/// 설계 원칙:
///   - MonoBehaviour를 상속하지 않는 순수 POCO 클래스이다.
///   - PlayerStateMachine이 생성자에서 인스턴스를 소유하며, OnStateChanged 이벤트를 통해 상태 변화를 수신한다.
///   - 지형 타입 판별은 PlayerSensor가 제공하는 IsOnPlatform 플래그와
///     별도로 주입된 ETerrainSoundType(씬 단위 기본 지형)으로 결정한다.
///   - Hover Loop 사운드는 HoverState Enter/Exit 시점에 Play/Stop 된다.
///   - Walk Loop 사운드는 Grounded 상태의 UpdateWalkSound 호출로 매 프레임 갱신된다.
/// </summary>
public class PlayerSoundHandler
{
    private readonly PlayerSensor _sensor;

    // 씬 기본 지형 타입 — PlayerStateMachine이 씬 진입 시 Set으로 주입한다.
    private ETerrainSoundType _defaultTerrainType = ETerrainSoundType.Scrap;

    private bool _isHoverLoopPlaying;

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
    // 상태별 진입/종료 SFX (PlayerStateMachine.OnStateChanged에서 호출)
    // ──────────────────────────────────────────

    public void OnEnterAirborne()
    {
        StopWalkLoop();
    }

    public void OnEnterGrounded()
    {
        PlayLandSfx();
    }

    public void OnEnterWallSlide()
    {
        StopWalkLoop();
        PlayHitWallSfx();
    }

    public void OnEnterHover()
    {
        StopWalkLoop();
        PlayHoverLoop();
    }

    public void OnExitHover()
    {
        StopHoverLoop();
    }

    public void OnEnterDash()
    {
        StopWalkLoop();
        PlayDashSfx();
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
            _walkSfxTimer = 0f;
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
        ESfxClip clip = ResolveWalkClip();
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayLandSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        ESfxClip clip = ResolveLandClip();
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayHitWallSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        ESfxClip clip = ResolveHitWallClip();
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayDashSfx()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }
        ESfxClip clip = ResolveDashClip();
        SoundManager.Instance.PlayPlayerSfx(clip);
    }

    private void PlayHoverLoop()
    {
        if (!SoundManager.HasInstance || _isHoverLoopPlaying)
        {
            return;
        }
        _isHoverLoopPlaying = true;
        SoundManager.Instance.PlayPlayerSfx(ESfxClip.SE_PC_Hovering);
    }

    private void StopHoverLoop()
    {
        _isHoverLoopPlaying = false;
    }

    private void StopWalkLoop()
    {
        _walkSfxTimer = 0f;
    }

    // ──────────────────────────────────────────
    // 지형 타입 → 클립 매핑
    // ──────────────────────────────────────────

    private ETerrainSoundType ResolveCurrentTerrainType()
    {
        if (_sensor.IsOnPlatform)
        {
            return ETerrainSoundType.Platform;
        }
        return _defaultTerrainType;
    }

    private ESfxClip ResolveWalkClip()
    {
        return ResolveCurrentTerrainType() switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_WalkingScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_WalkingMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_WalkingPlatform,
            _                          => ESfxClip.SE_PC_WalkingScrap,
        };
    }

    private ESfxClip ResolveLandClip()
    {
        return ResolveCurrentTerrainType() switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_LandScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_LandMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_LandPlatform,
            _                          => ESfxClip.SE_PC_LandScrap,
        };
    }

    private ESfxClip ResolveDashClip()
    {
        // 공중 대시는 별도 클립, 지상 대시는 지형별 클립 사용
        if (!_sensor.IsOnGround)
        {
            return ESfxClip.SE_PC_DashAir;
        }

        return ResolveCurrentTerrainType() switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_DashScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_DashMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_DashPlatform,
            _                          => ESfxClip.SE_PC_DashScrap,
        };
    }

    private ESfxClip ResolveHitWallClip()
    {
        return ResolveCurrentTerrainType() switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_HitWallScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_HitWallMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_HitWallPlatform,
            _                          => ESfxClip.SE_PC_HitWallScrap,
        };
    }
}
