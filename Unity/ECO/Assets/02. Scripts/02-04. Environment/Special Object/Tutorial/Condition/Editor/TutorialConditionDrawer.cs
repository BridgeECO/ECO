using System;
using UnityEditor;

/// <summary>
/// 튜토리얼 조건 항목에 타입 선택 드롭다운을 그린다. 그리기 본체는 SerializeReferenceDrawerBase에 있다.
///
/// 어트리뷰트가 아니라 타입에 등록하므로 TutorialObject의 조건 리스트가 [SubclassSelector] 없이도 걸린다.
/// </summary>
[CustomPropertyDrawer(typeof(TutorialConditionBase), true)]
public class TutorialConditionDrawer : SerializeReferenceDrawerBase
{
    private const string TYPE_NAME_PREFIX = "TC_";

    // 조건 리스트 안에서는 TC_ 접두사가 군더더기라 라벨에서만 뗀다.
    protected override string ToDisplayName(string typeName)
    {
        if (typeName.StartsWith(TYPE_NAME_PREFIX, StringComparison.Ordinal))
        {
            typeName = typeName.Substring(TYPE_NAME_PREFIX.Length);
        }

        return base.ToDisplayName(typeName);
    }
}
