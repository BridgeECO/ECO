using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ECO
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ResonanceObject : MonoBehaviour
    {
        public UnityEvent onResonanceActivate = new UnityEvent();

        [SerializeField] private Material _resonanceMat = null;

        private MaterialPropertyBlock _matPropBlock = null;
        private SpriteRenderer _spriteRenderer = null;
        public BoxCollider2D _boxCol = null;

        public bool isPlayed;

        public bool isAlwaysShowing;

        private void Awake()
        {
            if (!UNITY.TryGetComp(out _spriteRenderer, this.gameObject))
                return;
            if (!UNITY.TryGetComp(out _boxCol, this.gameObject))
                return;

            if(!isAlwaysShowing)
            {
                _spriteRenderer.material = _resonanceMat;

                _matPropBlock = new MaterialPropertyBlock();
                _matPropBlock.SetTexture("_MainTex", _spriteRenderer.sprite.texture);
                
                SetAlpha(0);
                _boxCol.enabled = false;
            }

            isPlayed = false;
        }

        public Bounds GetColBound()
        {
            return _boxCol.bounds;
        }

        public void ActivateResonance()
        {
            if(!isPlayed)
            {
                isPlayed = true;
                onResonanceActivate.Invoke();
            }
        }

        public void SetCircleParams(Vector2 centerPos, float radius)
        {
            if(isAlwaysShowing)
                return;
            
            if (radius <= 0)
                SetAlpha(0);
            else
                SetAlpha(1);

            _spriteRenderer.GetPropertyBlock(_matPropBlock);
            _matPropBlock.SetVector("_CenterPos", centerPos);
            _matPropBlock.SetFloat("_Radius", radius);
            _spriteRenderer.SetPropertyBlock(_matPropBlock);
        }

        public void SetAlpha(float alpha)
        {
            var color = _spriteRenderer.color;
            color.a = alpha;

            _spriteRenderer.color = color;
        }

        public IEnumerator ShowingTemp(float showingSpeed = 0.8f)
        {
            yield return null;

            float alpha = 0;

            _boxCol.enabled = true;

            while(alpha <= 1)
            {                
                alpha += showingSpeed * Time.deltaTime;
                SetAlpha(alpha);
                yield return null;
            }

            while(alpha >= 0)
            {                
                alpha -= showingSpeed * Time.deltaTime;
                SetAlpha(alpha);
                yield return null;
            }

            _boxCol.enabled = false;
        }
    }
}