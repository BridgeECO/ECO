using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Dialogue : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _dialogueCanvasgroup;

    [SerializeField]
    private TextMeshProUGUI _dialogueTextDisplay;

    [Foldout("Settings")]
    [SerializeField]
    private float _animationDuration = 0.4f;

    private void Awake()
    {
        if (_dialogueCanvasgroup != null)
        {
            _dialogueCanvasgroup.alpha = 0f;
            _dialogueCanvasgroup.transform.localScale = Vector3.zero;
        }
    }

    public void ShowDialogue(string text)
    {
        if (_dialogueTextDisplay != null)
        {
            _dialogueTextDisplay.text = text;
        }

        if (_dialogueCanvasgroup != null)
        {
            KillAllTween();
            _dialogueCanvasgroup.transform.localScale = Vector3.zero;
            _dialogueCanvasgroup.DOFade(1f, _animationDuration * 0.5f);
            _dialogueCanvasgroup.transform.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack);
        }
    }

    public void HideDialogue()
    {
        if (_dialogueCanvasgroup != null)
        {
            KillAllTween();
            _dialogueCanvasgroup.transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                _dialogueCanvasgroup.alpha = 0f;
            });
        }
    }

    private void KillAllTween()
    {
        _dialogueCanvasgroup.DOKill();
        _dialogueCanvasgroup.transform.DOKill();
    }
}
