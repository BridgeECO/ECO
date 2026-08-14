using UnityEngine;

/// <summary>
/// [SerializeReference] 필드·리스트에 파생 타입 선택 드롭다운을 붙인다.
///
/// 유니티가 기본 제공하는 타입 선택기는 UI Toolkit 인스펙터에만 붙는데, 이 프로젝트는
/// VInspector가 IMGUI로 인스펙터를 대체하고 있어 선택기가 따라오지 않는다.
/// 그래서 SubclassSelectorDrawer가 직접 그린다.
/// </summary>
public class SubclassSelectorAttribute : PropertyAttribute
{
}
