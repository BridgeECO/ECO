using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 맵 고정 위치에 조작 안내를 띄우는 World Space UI.
/// 인게임 로어를 반영하지 않는 시스템 메시지라 대화 UI(UI_Dialogue)와 분리한다.
///
/// 정적 안내는 스프라이트 한 장으로 충분하지만, 움직이는 안내가 필요하면 이미지 오브젝트에
/// UI_Reactor를 얹어 프로젝트 표준 연출 시스템에 재생을 맡긴다.
/// </summary>
public class UI_Tutorial : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _tutorialCanvasGroup;

    [SerializeField]
    private TextMeshProUGUI _tutorialTextDisplay;

    [SerializeField]
    private Image _tutorialImageDisplay;

    // 이미지 오브젝트에 붙인 리액터. 애니메이션을 재생하는 안내에서만 할당한다.
    [SerializeField]
    private UI_Reactor _imageReactor;

    [Foldout("Settings")]
    [SerializeField]
    private float _animationDuration = 0.4f;

    private void Awake()
    {
        if (_tutorialCanvasGroup != null)
        {
            _tutorialCanvasGroup.alpha = 0f;
            _tutorialCanvasGroup.transform.localScale = Vector3.zero;
        }
    }

    // 숨김 트윈의 OnComplete가 파괴된 CanvasGroup을 참조하지 않도록 먼저 끊는다.
    private void OnDestroy()
    {
        KillAllTween();
    }

    public void ShowTutorial(string text, Sprite image)
    {
        RefreshContent(text, image);
        PlayImageReaction();

        if (_tutorialCanvasGroup == null)
        {
            return;
        }

        KillAllTween();
        _tutorialCanvasGroup.transform.localScale = Vector3.zero;
        _tutorialCanvasGroup.DOFade(1f, _animationDuration * 0.5f);
        _tutorialCanvasGroup.transform.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack);
    }

    public void HideTutorial()
    {
        ExitImageReaction();

        if (_tutorialCanvasGroup == null)
        {
            return;
        }

        KillAllTween();
        _tutorialCanvasGroup.transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            _tutorialCanvasGroup.alpha = 0f;
        });
    }

    /// <summary>
    /// 연출 없이 즉시 숨긴다. 비활성화나 리셋처럼 플레이어가 보고 있지 않고
    /// 트윈의 완료 콜백을 기다릴 수도 없는 시점에 사용한다.
    /// </summary>
    public void HideTutorialImmediate()
    {
        KillAllTween();
        StopImageReaction();

        if (_tutorialCanvasGroup == null)
        {
            return;
        }

        _tutorialCanvasGroup.alpha = 0f;
        _tutorialCanvasGroup.transform.localScale = Vector3.zero;
    }

    private void RefreshContent(string text, Sprite image)
    {
        if (_tutorialTextDisplay != null)
        {
            _tutorialTextDisplay.text = text;
        }

        if (_tutorialImageDisplay == null)
        {
            return;
        }

        // 이미지는 선택 사항이다. 지정되지 않은 튜토리얼에서 빈 사각형이 남지 않도록 오브젝트째 끈다.
        // 리액터가 있으면 스프라이트가 비어 있어도 애니메이션이 자리를 채우므로 켜 둔다.
        bool hasImage = image != null || _imageReactor != null;
        _tutorialImageDisplay.gameObject.SetActive(hasImage);
        if (image != null)
        {
            _tutorialImageDisplay.sprite = image;
        }
    }

    private void PlayImageReaction()
    {
        if (_imageReactor == null)
        {
            return;
        }

        _imageReactor.PlaySignalAsync(EUIReactionSignal.Show, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void ExitImageReaction()
    {
        if (_imageReactor == null)
        {
            return;
        }

        _imageReactor.PlaySignalExitAsync(EUIReactionSignal.Show, this.GetCancellationTokenOnDestroy()).Forget();
    }

    // 오브젝트를 끄면 UI_Reactor가 자기 토큰을 끊어 재생이 즉시 멈춘다.
    // 그냥 두면 루프 애니메이션이 알파 0 뒤에서 계속 돈다.
    private void StopImageReaction()
    {
        if (_imageReactor == null || _tutorialImageDisplay == null)
        {
            return;
        }

        _tutorialImageDisplay.gameObject.SetActive(false);
    }

    private void KillAllTween()
    {
        if (_tutorialCanvasGroup == null)
        {
            return;
        }

        _tutorialCanvasGroup.DOKill();
        _tutorialCanvasGroup.transform.DOKill();
    }
}
