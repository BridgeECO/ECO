using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// [SerializeReference] 항목에 타입 선택 드롭다운과 자식 필드를 그리는 공통 본체.
/// 유니티 기본 선택기는 UI Toolkit 전용이라, IMGUI로 인스펙터를 대체하는 VInspector에서는 따라오지 않는다.
/// </summary>
// 이 클래스에는 [CustomPropertyDrawer]를 절대 달지 않는다.
// 어트리뷰트는 상속되므로 파생 드로어가 베이스의 타깃에도 함께 등록되어 엉뚱한 프로퍼티를 가로챈다.
public abstract class SerializeReferenceDrawerBase : PropertyDrawer
{
    // 드롭다운이 아무리 좁아도 이만큼은 남긴다. 라벨이 폭을 다 먹으면 타입을 고를 수 없다.
    private const float MIN_DROPDOWN_WIDTH = 60f;

    private Func<string, string> _toDisplayName;

    // 드롭다운 라벨은 리페인트마다 계산되므로 대리자를 매번 새로 만들지 않는다.
    private Func<string, string> DisplayNameFunc
    {
        get
        {
            if (_toDisplayName == null)
            {
                _toDisplayName = ToDisplayName;
            }

            return _toDisplayName;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float labelWidth = Mathf.Clamp(GetLabelWidth(position), 0f, Mathf.Max(0f, position.width - MIN_DROPDOWN_WIDTH));
        Rect labelRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
        Rect dropdownRect = new Rect(position.x + labelWidth, position.y,
            position.width - labelWidth, EditorGUIUtility.singleLineHeight);

        GUIContent headerLabel = GetHeaderLabel(property, label);

        bool hasValue = SerializeReferenceTypeMenu.HasValue(property);
        if (hasValue)
        {
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, headerLabel, true);
        }
        else
        {
            EditorGUI.LabelField(labelRect, headerLabel);
        }

        string dropdownText = SerializeReferenceTypeMenu.GetDisplayName(property, DisplayNameFunc);
        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(dropdownText), FocusType.Keyboard))
        {
            SerializeReferenceTypeMenu.Show(property, GetBaseType(),
                type => IsTypeSelectable(type, property), DisplayNameFunc);
        }

        if (hasValue && property.isExpanded && IsDrawChildren(property))
        {
            EditorGUI.indentLevel++;
            DrawChildFields(position, property);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (!SerializeReferenceTypeMenu.HasValue(property) || !property.isExpanded || !IsDrawChildren(property))
        {
            return height;
        }

        foreach (SerializedProperty child in EnumerateChildren(property))
        {
            height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    #region Hooks
    /// <summary>폴드아웃 왼쪽에 적을 내용. 기본은 유니티가 준 라벨 그대로다.</summary>
    protected virtual GUIContent GetHeaderLabel(SerializedProperty property, GUIContent label)
    {
        return label;
    }

    /// <summary>라벨이 차지할 폭. 요약 줄을 그리는 드로어는 더 넓게 잡는다.</summary>
    protected virtual float GetLabelWidth(Rect position)
    {
        return EditorGUIUtility.labelWidth;
    }

    /// <summary>타입 이름을 사람이 읽을 형태로 바꾼다. 접두사를 떼려는 드로어가 여기를 덮는다.</summary>
    protected virtual string ToDisplayName(string typeName)
    {
        return ObjectNames.NicifyVariableName(typeName);
    }

    /// <summary>false를 돌려준 타입은 선택 메뉴에서 감춘다. 이미 선택된 타입은 이 판정과 무관하게 남는다.</summary>
    protected virtual bool IsTypeSelectable(Type type, SerializedProperty property)
    {
        return true;
    }

    /// <summary>펼쳤을 때 자식 필드를 그릴지. 중첩이 너무 깊은 자리를 막는 드로어가 여기를 덮는다.</summary>
    protected virtual bool IsDrawChildren(SerializedProperty property)
    {
        return true;
    }
    #endregion

    // 자식 필드는 이 드로어의 타깃이 아니므로 기본 드로어로 그려도 여기로 되돌아오지 않는다.
    // 자식이 다시 [SerializeReference]라면 그때는 그 타입에 등록된 드로어가 새로 걸린다.
    private void DrawChildFields(Rect position, SerializedProperty property)
    {
        float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        foreach (SerializedProperty child in EnumerateChildren(property))
        {
            float fieldHeight = EditorGUI.GetPropertyHeight(child, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, fieldHeight), child, true);
            y += fieldHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    /// <summary>
    /// 높이 계산과 실제 그리기가 반드시 같은 순서·같은 개수를 훑어야 한다.
    /// 두 벌로 두면 한쪽만 고쳐졌을 때 인스펙터 레이아웃이 어긋난다.
    /// </summary>
    private static IEnumerable<SerializedProperty> EnumerateChildren(SerializedProperty property)
    {
        SerializedProperty iterator = property.Copy();
        SerializedProperty end = property.GetEndProperty();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            enterChildren = false;
            yield return iterator;
        }
    }

    /// <summary>
    /// 리스트·배열이면 원소 타입이, 단일 필드면 그 타입이 후보의 기준이 된다.
    ///
    /// 중첩 [SerializeReference]에서도 fieldInfo가 잡히는 것은, 유니티가
    /// isReferencingAManagedReferenceField일 때 프로퍼티 경로를 가장 안쪽 관리 참조의
    /// 런타임 타입 기준으로 다시 푸는 덕분이다. 정적 경로 탐색만으로는 파생 클래스에만
    /// 선언된 필드를 찾지 못한다.
    /// </summary>
    private Type GetBaseType()
    {
        Type fieldType = fieldInfo?.FieldType;
        if (fieldType == null)
        {
            return null;
        }

        if (fieldType.IsArray)
        {
            return fieldType.GetElementType();
        }

        if (fieldType.IsGenericType)
        {
            Type[] arguments = fieldType.GetGenericArguments();
            return arguments.Length == 1 ? arguments[0] : null;
        }

        return fieldType;
    }
}
