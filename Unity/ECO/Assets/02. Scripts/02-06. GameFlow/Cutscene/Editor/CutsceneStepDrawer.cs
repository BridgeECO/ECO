using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 컷씬 스텝 항목을 그린다. 어트리뷰트가 아니라 타입에 등록하므로
/// 그룹 스텝 안의 자식 리스트에도 같은 드로어가 그대로 걸린다.
/// </summary>
[CustomPropertyDrawer(typeof(CutsceneStepBase), true)]
public class CutsceneStepDrawer : SerializeReferenceDrawerBase
{
    private const string ARRAY_TOKEN = ".Array.data[";
    private const float DROPDOWN_WIDTH = 170f;

    private static readonly GUIContent _tooDeepLabel = new GUIContent("⚠ 중첩이 너무 깊습니다");

    // 리페인트마다 새로 만들지 않는다.
    private static readonly GUIContent _headerLabel = new GUIContent();

    protected override string ToDisplayName(string typeName)
    {
        return CutsceneStepSummary.GetDisplayName(typeName);
    }

    protected override GUIContent GetHeaderLabel(SerializedProperty property, GUIContent label)
    {
        if (!IsDrawChildren(property))
        {
            return _tooDeepLabel;
        }

        _headerLabel.text = CutsceneStepSummary.Build(property);
        return _headerLabel;
    }

    // 요약 줄이 라벨 자리에 들어가므로 labelWidth(약 150px)로 자르면 뒷부분이 통째로 사라진다.
    protected override float GetLabelWidth(Rect position)
    {
        return Mathf.Max(EditorGUIUtility.labelWidth, position.width - DROPDOWN_WIDTH);
    }

    // 상한에 닿으면 그룹 계열을 목록에서 아예 뺀다. 만들고 나서 경고하는 것보다 못 만들게 막는 편이 싸다.
    protected override bool IsTypeSelectable(Type type, SerializedProperty property)
    {
        if (!typeof(CutsceneGroupStepBase).IsAssignableFrom(type))
        {
            return true;
        }

        return GetNestDepth(property) + 1 < CutsceneGroupStepBase.MAX_NEST_DEPTH;
    }

    protected override bool IsDrawChildren(SerializedProperty property)
    {
        return GetNestDepth(property) < CutsceneGroupStepBase.MAX_NEST_DEPTH;
    }

    /// <summary>
    /// "_steps.Array.data[0]._steps.Array.data[1]" 처럼 리스트를 지날 때마다 한 단씩 깊어진다.
    /// 최상위 리스트가 1이므로 그룹 안의 자식은 2부터 시작한다.
    /// </summary>
    private static int GetNestDepth(SerializedProperty property)
    {
        string path = property.propertyPath;
        int depth = 0;
        int index = path.IndexOf(ARRAY_TOKEN, StringComparison.Ordinal);
        while (0 <= index)
        {
            depth++;
            index = path.IndexOf(ARRAY_TOKEN, index + ARRAY_TOKEN.Length, StringComparison.Ordinal);
        }

        return depth;
    }
}
