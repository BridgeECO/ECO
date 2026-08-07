using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 팝업 개폐 스케일 연출. Unity 생명주기가 필요 없어 컴포넌트가 아닌
/// 직렬화 클래스로 두고, UI_Popup이 인라인 필드로 소유하며 대상 Transform을 넘겨 재생한다.
/// </summary>
[Serializable]
public class UI_ScaleAnimator
{
    [SerializeField]
    private float _openDuration = 0.3f;

    [SerializeField]
    private float _closeDuration = 0.2f;

    [SerializeField]
    private Ease _openEase = Ease.OutBack;

    [SerializeField]
    private Ease _closeEase = Ease.InBack;

    [SerializeField]
    private Vector3 _startScale = new Vector3(0.5f, 0.5f, 0.5f);

    [SerializeField]
    private Vector3 _targetScale = Vector3.one;

    public async UniTask PlayOpenAsync(Transform target, CancellationToken ct)
    {
        target.localScale = _startScale;
        await target.DOScale(_targetScale, _openDuration)
            .SetEase(_openEase)
            .SetUpdate(true)
            .ToUniTask(cancellationToken: ct);
    }

    public async UniTask PlayCloseAsync(Transform target, CancellationToken ct)
    {
        target.localScale = _targetScale;
        await target.DOScale(_startScale, _closeDuration)
            .SetEase(_closeEase)
            .SetUpdate(true)
            .ToUniTask(cancellationToken: ct);
    }
}
