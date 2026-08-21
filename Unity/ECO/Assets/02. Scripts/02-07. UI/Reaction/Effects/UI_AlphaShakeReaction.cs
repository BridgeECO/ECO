using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 투명도를 랜덤하게 떨리게 해 불규칙한 명멸을 만든다. 통신 두절이나 경고 표시에 쓴다.
/// 숨쉬듯 규칙적으로 밝아졌다 어두워지는 연출은 Alpha 리액션에 반복을 걸면 된다.
/// </summary>
[Serializable]
[Preserve]
public class UI_AlphaShakeReaction : UI_ShakeReactionBase
{
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("떨림의 진폭입니다. 배율이 아니라 기준 투명도에 더해지는 절대량입니다.")]
    private float _strength = 0.4f;

    private CanvasGroup _canvasGroup;
    private Graphic _graphic;

    private DOGetter<Vector3> _getter;
    private DOSetter<Vector3> _setter;

    public override EUIReactionChannel Channel => EUIReactionChannel.Alpha;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target))
        {
            return;
        }

        float baseline = EnsureBaseline(context, target, new Vector4(ReadAlpha(), 0f, 0f, 0f)).x;

        if (IsStartFromBaseline)
        {
            WriteAlpha(baseline);
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target))
        {
            return null;
        }

        EnsureBaseline(context, target, new Vector4(ReadAlpha(), 0f, 0f, 0f));
        EnsureAccessors();

        // DOTween에는 float용 흔들림이 없다. Vector3 흔들림의 x축만 알파에 물려 쓴다.
        return DOTween.Shake(_getter, _setter, Motion.Duration, new Vector3(_strength, 0f, 0f),
            Vibrato, Randomness, IsFadeOut);
    }

    protected override Tween CreateExitTween(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target) || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return null;
        }

        return _canvasGroup != null
            ? _canvasGroup.DOFade(baseline.x, Motion.Duration)
            : _graphic.DOFade(baseline.x, Motion.Duration);
    }

    protected override void ApplyBaseline(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target) || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return;
        }

        WriteAlpha(baseline.x);
    }

    // 재생할 때마다 새로 잡으면 델리게이트가 그대로 쓰레기가 된다.
    // [SerializeReference] 역직렬화 경로에서 초기화가 도는지에 기대지 않고 지연 생성한다.
    private void EnsureAccessors()
    {
        if (_getter != null)
        {
            return;
        }

        _getter = GetAlphaVector;
        _setter = SetAlphaVector;
    }

    private Vector3 GetAlphaVector()
    {
        return new Vector3(ReadAlpha(), 0f, 0f);
    }

    // 기준 투명도가 1이면 위로 넘치는 절반이 잘려 나가, 대부분 켜져 있다가 불규칙하게 툭툭 꺼지는
    // 모양이 된다. 깜빡임 연출에는 이쪽이 자연스러워 그대로 둔다.
    private void SetAlphaVector(Vector3 value)
    {
        WriteAlpha(Mathf.Clamp01(value.x));
    }

    private bool TryResolve(GameObject target)
    {
        _canvasGroup = target.GetComponent<CanvasGroup>();
        _graphic = _canvasGroup != null ? null : target.GetComponent<Graphic>();
        return _canvasGroup != null || _graphic != null;
    }

    private float ReadAlpha()
    {
        return _canvasGroup != null ? _canvasGroup.alpha : _graphic.color.a;
    }

    private void WriteAlpha(float alpha)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = alpha;
            return;
        }

        Color color = _graphic.color;
        color.a = alpha;
        _graphic.color = color;
    }
}
