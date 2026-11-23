using UnityEngine;
using VInspector;

/// <summary>
/// 버튼식 특수 오브젝트가 상호작용 대상으로 선택된 동안 외곽선과 키 표시를 함께 켠다.
/// 선택 판정은 SpecialObjectBase가, 보이기만 이 컴포넌트가 맡는다.
/// </summary>
public class InteractionPromptView : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private SpecialObjectBase _target;

    [SerializeField]
    private GameObject _outlineObject;

    [SerializeField]
    private GameObject _keyPromptObject;

    private void OnEnable()
    {
        _target.OnSelectedChanged += RefreshPrompt;
        RefreshPrompt(_target.IsSelected);
    }

    private void OnDisable()
    {
        _target.OnSelectedChanged -= RefreshPrompt;
        RefreshPrompt(false);
    }

    private void RefreshPrompt(bool isSelected)
    {
        _outlineObject.SetActive(isSelected);
        _keyPromptObject.SetActive(isSelected);
    }
}
