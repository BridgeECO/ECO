using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("연결 설정")]
    [Tooltip("빛 효과 쉐이더가 적용된 Image를 여기에 드래그해 넣으세요.")]
    public Image glowEffectImage;

    [Header("모션 타임라인 설정")]
    [Tooltip("애니메이션 시간 (초). 60프레임 기준 3프레임은 약 0.05초, 5프레임은 약 0.08초 입니다.")]
    public float duration = 0.08f;

    [Tooltip("모션그래픽의 쫀득함을 살려줄 이징(Easing) 커브. (추천: 좌하단에서 우상단으로 가는 S자 곡선)")]
    public AnimationCurve motionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("크기(Scale) 애니메이션 설정")]
    [Tooltip("마우스가 올라갔을 때(완전히 켜졌을 때)의 크기 비율")]
    public Vector2 hoverScale = new Vector2(1f, 1f);

    [Tooltip("사라졌을 때의 크기 비율. (0.5, 0) 처럼 세팅하면 가로도 살짝 줄고, 위아래로 완전히 납작해지며 사라집니다.")]
    public Vector2 normalScale = new Vector2(0.5f, 0f);

    private Material instancedMaterial;
    private RectTransform effectRect;

    private float transitionProgress = 0f; // 0.0(꺼짐) ~ 1.0(켜짐) 사이의 진행도
    private bool isHovering = false;

    void Start()
    {
        if (glowEffectImage != null)
        {
            effectRect = glowEffectImage.rectTransform;

            if (glowEffectImage.material != null)
            {
                // 원본 머티리얼을 오염시키지 않기 위해 인스턴스화
                instancedMaterial = Instantiate(glowEffectImage.material);
                glowEffectImage.material = instancedMaterial;

                // 시작 시 투명도 0으로 초기화
                instancedMaterial.SetFloat("_HoverAlpha", 0f);
            }

            // 시작 시 크기를 꺼진 상태(normalScale)로 초기화
            effectRect.localScale = new Vector3(normalScale.x, normalScale.y, 1f);
        }
    }

    void Update()
    {
        if (instancedMaterial == null || effectRect == null) return;

        // 1. 목표 시간에 맞춰 진행도(Progress)를 0~1 사이로 계산합니다.
        if (isHovering && transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / duration;
            if (transitionProgress > 1f) transitionProgress = 1f;
        }
        else if (!isHovering && transitionProgress > 0f)
        {
            transitionProgress -= Time.deltaTime / duration;
            if (transitionProgress < 0f) transitionProgress = 0f;
        }

        // 진행도가 0이나 1에 도달해 멈춰있을 때는 불필요한 연산을 스킵합니다.
        if (!isHovering && transitionProgress == 0f) return;
        if (isHovering && transitionProgress == 1f) return;

        // 2. 진행도(0~1)를 Animation Curve에 넣어 쫀득한 가감속 값을 뽑아냅니다.
        float curveValue = motionCurve.Evaluate(transitionProgress);

        // 3. 투명도(Alpha) 부드럽게 적용
        instancedMaterial.SetFloat("_HoverAlpha", curveValue);

        // 4. 가로/세로 크기(Scale) 쫀득하게 적용
        Vector2 currentScale = Vector2.Lerp(normalScale, hoverScale, curveValue);
        effectRect.localScale = new Vector3(currentScale.x, currentScale.y, 1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}