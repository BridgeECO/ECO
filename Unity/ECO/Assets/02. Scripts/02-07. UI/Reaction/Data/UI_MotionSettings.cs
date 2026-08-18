using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 모션 리액션이 공유하는 재생 설정. 트윈 리액션마다 같은 필드를 다시 쓰지 않도록 묶어 둔다.
/// </summary>
[Serializable]
public class UI_MotionSettings
{
    [SerializeField]
    private float _duration = 0.15f;

    [SerializeField]
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
    public bool IsInfiniteLoop => _loops < 0;

    public Tween ApplyTo(Tween tween)
    {
        if (tween == null)
        {
            return null;
        }

        // 커브에 키가 하나도 없으면 계속 0을 반환해 대상이 시작값에 붙어 버린다.
        if (_isUseCurve && _curve != null && 0 < _curve.length)
        {
            tween.SetEase(_curve);
        }
        else
        {
            tween.SetEase(_ease);
        }

        if (0f < _delay)
        {
            tween.SetDelay(_delay);
        }

        if (_loops != 1)
        {
            tween.SetLoops(_loops, _loopType);
        }

        // 재활용을 켠다. 이 프로젝트의 전역 기본값은 꺼짐이라 그대로 두면 hover가 오갈 때마다
        // 트윈 객체가 새로 할당된다. 보관 중인 핸들이 남의 트윈을 가리킬 위험은
        // UI_TweenPolicyRunner가 OnKill에서 핸들을 비워 막는다.
        return tween.SetUpdate(_isIgnoreTimeScale).SetRecyclable(true);
    }
}
