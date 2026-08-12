using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 맵 고정 위치에 조작 안내를 띄우는 World Space UI.
/// 인게임 로어를 반영하지 않는 시스템 메시지라 대화 UI(UI_Dialogue)와 분리한다.
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
        bool hasImage = image != null;
        _tutorialImageDisplay.gameObject.SetActive(hasImage);
        if (hasImage)
        {
            _tutorialImageDisplay.sprite = image;
        }
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
