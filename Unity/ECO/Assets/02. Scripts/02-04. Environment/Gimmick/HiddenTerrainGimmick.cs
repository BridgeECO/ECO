using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HiddenTerrainGimmick : TerrainGimmickBase
{
    private float _fadeDuration;
    private float _detectionPadding;
    private CancellationTokenSource _fadeCts;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    public HiddenTerrainGimmick(
        EGimmickActivationType activationType,
        bool isInverted,
        float fadeDuration,
        float detectionPadding)
        : base(activationType, isInverted)
    {
        _fadeDuration = fadeDuration;
        _detectionPadding = detectionPadding;
    }

    public override void OnTerrainTriggerEnter2D(Collider2D other)
    {
        if (!IsActivated || !other.CompareTag(nameof(ETags.Player)) || _spriteRenderer == null)
        {
            return;
        }
        StartFade(_spriteRenderer, 0f);
    }

    public override void OnTerrainTriggerExit2D(Collider2D other)
    {
        if (!IsActivated || !other.CompareTag(nameof(ETags.Player)) || _spriteRenderer == null)
        {
            return;
        }
        StartFade(_spriteRenderer, 1f);
    }

    public override void OnDestroy(TerrainObject target)
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = null;
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        if (!TryCacheComponents(target))
        {
            return;
        }
        UpdateLayerAndTrigger(target, isActivated);
        UpdateColliderSize(target, isActivated);
    }

    private bool TryCacheComponents(TerrainObject target)
    {
        if (_spriteRenderer == null || _boxCollider == null)
        {
            _spriteRenderer = target.GetComponent<SpriteRenderer>();
            _boxCollider = target.GetComponent<BoxCollider2D>();
        }
        return _spriteRenderer != null && _boxCollider != null;
    }

    private void UpdateLayerAndTrigger(TerrainObject target, bool isActivated)
    {
        target.gameObject.layer = isActivated ? (int)ELayers.GimmickActivation : (int)ELayers.Terrain;
        _boxCollider.isTrigger = isActivated;
    }

    private void UpdateColliderSize(TerrainObject target, bool isActivated)
    {
        Vector2 spriteLocalSize = _spriteRenderer.sprite.bounds.size;

        if (isActivated)
        {
            Vector3 scale = target.transform.lossyScale;
            _boxCollider.size = new Vector2(
                spriteLocalSize.x + (_detectionPadding * 2f) / Mathf.Abs(scale.x),
                spriteLocalSize.y + (_detectionPadding * 2f) / Mathf.Abs(scale.y));
        }
        else
        {
            _boxCollider.size = spriteLocalSize;
            SetSpriteAlpha(_spriteRenderer, 1f);
        }
        _boxCollider.offset = Vector2.zero;
    }

    private void StartFade(SpriteRenderer spriteRenderer, float targetAlpha)
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = new CancellationTokenSource();
        FadeAsync(spriteRenderer, targetAlpha, _fadeCts.Token).Forget();
    }

    private async UniTaskVoid FadeAsync(SpriteRenderer spriteRenderer, float targetAlpha, CancellationToken ct)
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeDuration);
            SetSpriteAlpha(spriteRenderer, Mathf.Lerp(startAlpha, targetAlpha, t));
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        SetSpriteAlpha(spriteRenderer, targetAlpha);
    }

    private void SetSpriteAlpha(SpriteRenderer spriteRenderer, float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
