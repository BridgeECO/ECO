using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

/// <summary>
/// 버튼보다 넓은 영역을 hover 판정에 쓰고 싶을 때 그 영역에 붙인다.
///
/// 포인터 진입·이탈만 구현한다. 이 둘은 EventSystem이 조상 전체에 전달하므로 아무것도
/// 가로채지 않는다. 반면 Down·Click은 처음 만난 핸들러에서 멈추기 때문에, 여기서
/// 그 인터페이스들을 구현하면 부모 버튼의 클릭이 죽는다.
/// </summary>
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
