using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

public class UI_ScaleAnimator : MonoBehaviour
{
    [SerializeField]
    private float _openDuration = 0.3f;

    [SerializeField]
    private float _closeDuration = 1.0f;

    [SerializeField]
    private Ease _openEase = Ease.OutBack;

    [SerializeField]
    private Ease _closeEase = Ease.InBack;

    [SerializeField]
    private Vector3 _startScale = new Vector3(0.5f, 0.5f, 0.5f);

    [SerializeField]
    private Vector3 _targetScale = Vector3.one;

    public async UniTask PlayOpenAsync(CancellationToken ct)
    {
        transform.localScale = _startScale;
        await transform.DOScale(_targetScale, _openDuration)
            .SetEase(_openEase)
            .SetUpdate(true)
            .ToUniTask(cancellationToken: ct);
    }

    public async UniTask PlayCloseAsync(CancellationToken ct)
    {
        transform.localScale = _targetScale;
        await transform.DOScale(_startScale, _closeDuration)
            .SetEase(_closeEase)
            .SetUpdate(true)
            .ToUniTask(cancellationToken: ct);
    }
}
