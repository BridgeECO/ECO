using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

/// <summary>
/// 버튼보다 넓은 영역을 hover 판정에 쓰고 싶을 때 그 영역에 붙인다.
/// </summary>
// 진입·이탈만 구현한다. Down·Click은 처음 만난 핸들러에서 멈추므로, 여기서 구현하면 부모 버튼의 클릭이 죽는다.
public class UI_ReactionRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Reactor _reactor;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_reactor == null)
        {
            return;
        }

        _reactor.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_reactor == null)
        {
            return;
        }

        _reactor.OnPointerExit(eventData);
    }
}
