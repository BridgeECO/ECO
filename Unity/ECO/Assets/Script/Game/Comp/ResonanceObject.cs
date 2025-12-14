using UnityEngine;

namespace ECO
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ResonanceObject : MonoBehaviour
    {
        [SerializeField] private Material _resonanceMat = null;

        private MaterialPropertyBlock _matPropBlock = null;
        private SpriteRenderer _spriteRenderer = null;
        private BoxCollider2D _boxCol = null;

        private void Awake()
        {
            if (!UNITY.TryGetComp(out _spriteRenderer, this.gameObject))
                return;
            if (!UNITY.TryGetComp(out _boxCol, this.gameObject))
                return;

            _spriteRenderer.material = _resonanceMat;

            //var color = _spriteRenderer.color;
            //color.a = 0;

            //_spriteRenderer.color = color;
            _matPropBlock = new MaterialPropertyBlock();
        }

        public Bounds GetColBound()
        {
            return _boxCol.bounds;
        }

        public void SetCircleParams(Vector2 centerPos, float radius)
        {
            _spriteRenderer.GetPropertyBlock(_matPropBlock);

            _matPropBlock.SetTexture("_MainTex", _spriteRenderer.sprite.texture);
            _matPropBlock.SetVector("_CenterPos", centerPos);
            _matPropBlock.SetFloat("_Radius", radius);

            _spriteRenderer.SetPropertyBlock(_matPropBlock);
        }
    }
}