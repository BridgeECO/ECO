using System;
using DG.Tweening;
using UnityEngine;

/// <summary>모션 리액션이 공유하는 재생 설정. 트윈 리액션마다 같은 필드를 다시 쓰지 않도록 묶어 둔다.</summary>
[Serializable]
public class UI_MotionSettings
{
    // 음수가 그대로 DOTween에 넘어가면 트윈이 무엇을 할지 예측할 수 없다. 인스펙터에서 막는다.
    [SerializeField]
    [Min(0f)]
    private float _duration = 0.15f;

    [SerializeField]
    [Min(0f)]
    private float _delay = 0f;

    [SerializeField]
    private EUIReactionValueMode _valueMode = EUIReactionValueMode.RelativeToBaseline;

    [SerializeField]
    private bool _isUseCurve = false;

    [SerializeField]
    private Ease _ease = Ease.OutQuad;

    [SerializeField]
    private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    [Tooltip("1이면 한 번, -1이면 무한 반복입니다.")]
    private int _loops = 1;

    [SerializeField]
    private LoopType _loopType = LoopType.Yoyo;

    [SerializeField]
    [Tooltip("일시정지 메뉴가 timeScale을 0으로 만들기 때문에 UI 연출은 보통 켜 둡니다.")]
    private bool _isIgnoreTimeScale = true;

    public float Duration => _duration;
    public float Delay => _delay;
    public EUIReactionValueMode ValueMode => _valueMode;

    /// <summary>무한 반복은 끝나지 않으므로 완료를 기다리는 정책에서는 즉시 완료로 취급한다.</summary>
    public bool IsInfiniteLoop => Loops < 0;

    // 기획자가 0을 넣으면 "반복 없음"을 뜻한 것이지 "0번 재생"이 아니다. 1회 재생으로 읽는다.
    private int Loops => _loops == 0 ? 1 : _loops;

    public Tween ApplyTo(Tween tween)
    {
        if (tween == null)
        {
            return null;
        }

        ApplyEase(tween);

        if (0f < _delay)
        {
            tween.SetDelay(_delay);
        }

        if (Loops != 1)
        {
            tween.SetLoops(Loops, _loopType);
        }

        return ApplyCommon(tween);
    }

    /// <summary>
    /// 물러날 때 쓰는 트윈. 이즈와 시간 축만 물려주고 Delay와 Loops는 뺀다.
    /// 진입을 늦추려고 넣은 Delay가 퇴장에도 걸리면 포인터를 뗀 뒤 그만큼 이전 값이 남아 반응이 굼떠 보이고,
    /// Loops가 걸리면 기준값 복귀가 왕복 횟수만큼 늦어진다. 둘 다 진입 연출을 꾸미려던 설정이다.
    /// </summary>
    public Tween ApplyToExit(Tween tween)
    {
        if (tween == null)
        {
            return null;
        }

        ApplyEase(tween);
        return ApplyCommon(tween);
    }

    private void ApplyEase(Tween tween)
    {
        // 커브에 키가 하나도 없으면 계속 0을 반환해 대상이 시작값에 붙어 버린다.
        if (_isUseCurve && _curve != null && 0 < _curve.length)
        {
            tween.SetEase(_curve);
        }
        else
        {
            tween.SetEase(_ease);
        }
    }

    // 재활용을 켠다. 전역 기본값이 꺼짐이라 그대로 두면 hover가 오갈 때마다 트윈이 새로 할당된다.
    // 핸들이 남의 트윈을 가리킬 위험은 UI_TweenPolicyRunner가 OnKill에서 비워 막는다.
    private Tween ApplyCommon(Tween tween)
    {
        return tween.SetUpdate(_isIgnoreTimeScale).SetRecyclable(true);
    }
}
