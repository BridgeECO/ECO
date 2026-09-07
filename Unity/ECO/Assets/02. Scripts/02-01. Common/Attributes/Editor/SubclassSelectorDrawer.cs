using UnityEditor;

/// <summary>
/// [SubclassSelector]가 붙은 [SerializeReference] 항목에 타입 선택 드롭다운을 그린다.
/// 그리기 본체는 SerializeReferenceDrawerBase에 있다.
/// </summary>
[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : SerializeReferenceDrawerBase
{
}
