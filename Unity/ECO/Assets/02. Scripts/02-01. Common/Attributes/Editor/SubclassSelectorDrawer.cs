using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// [SubclassSelector]가 붙은 [SerializeReference] 항목에 타입 선택 드롭다운을 그린다.
///
/// 유니티 기본 선택기는 UI Toolkit 인스펙터 전용인데 이 프로젝트는 VInspector가 IMGUI로
/// 인스펙터를 대체한다. 선택기가 따라오지 않아 리스트에 항목을 추가해도 타입을 고를 방법이
/// 없고 빈 줄만 생기므로 직접 그린다.
/// </summary>
[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    private const string EMPTY_LABEL = "(타입 선택)";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect labelRect = new Rect(headerRect.x, headerRect.y, EditorGUIUtility.labelWidth, headerRect.height);
        Rect dropdownRect = new Rect(headerRect.x + EditorGUIUtility.labelWidth, headerRect.y,
            headerRect.width - EditorGUIUtility.labelWidth, headerRect.height);

        bool hasValue = HasValue(property);
        if (hasValue)
        {
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
        }
        else
        {
            EditorGUI.LabelField(labelRect, label);
        }

        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(GetTypeDisplayName(property)), FocusType.Keyboard))
        {
            ShowTypeMenu(property);
        }

        if (hasValue && property.isExpanded)
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
        if (!HasValue(property) || !property.isExpanded)
        {
            return height;
        }

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = property.GetEndProperty();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            enterChildren = false;
            height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    // 자식 필드는 이 어트리뷰트를 달고 있지 않으므로 기본 드로어로 그려도 여기로 되돌아오지 않는다.
    private void DrawChildFields(Rect position, SerializedProperty property)
    {
        float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = property.GetEndProperty();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            enterChildren = false;
            float fieldHeight = EditorGUI.GetPropertyHeight(iterator, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, fieldHeight), iterator, true);
            y += fieldHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private void ShowTypeMenu(SerializedProperty property)
    {
        Type baseType = GetBaseType();
        if (baseType == null)
        {
            return;
        }

        // 메뉴 콜백은 이 프레임 이후에 실행되므로 SerializedProperty를 그대로 붙들면 무효화된다.
        // 경로와 SerializedObject만 넘겨 그 시점에 다시 찾는다.
        SerializedObject serializedObject = property.serializedObject;
        string propertyPath = property.propertyPath;
        Type currentType = property.managedReferenceValue?.GetType();

        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("None"), currentType == null,
            () => SetValueType(serializedObject, propertyPath, null));
        menu.AddSeparator(string.Empty);

        foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
        {
            if (type.IsAbstract || type.IsGenericTypeDefinition || type.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            Type selectedType = type;
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), currentType == type,
                () => SetValueType(serializedObject, propertyPath, selectedType));
        }

        menu.ShowAsContext();
    }

    /// <summary>리스트·배열이면 원소 타입이, 단일 필드면 그 타입이 후보의 기준이 된다.</summary>
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

    private static void SetValueType(SerializedObject serializedObject, string propertyPath, Type valueType)
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.FindProperty(propertyPath);
        if (property == null)
        {
            return;
        }

        // 고른 타입이 지금 것과 같으면 손대지 않는다. 새 인스턴스로 갈아치우면
        // 기획자가 입력해 둔 수치와 참조가 그대로 사라진다.
        if (property.managedReferenceValue?.GetType() == valueType)
        {
            return;
        }

        property.managedReferenceValue = valueType == null ? null : Activator.CreateInstance(valueType);
        property.isExpanded = true;
        serializedObject.ApplyModifiedProperties();
    }

    private static bool HasValue(SerializedProperty property)
    {
        return !string.IsNullOrEmpty(property.managedReferenceFullTypename);
    }

    private static string GetTypeDisplayName(SerializedProperty property)
    {
        string fullTypename = property.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(fullTypename))
        {
            return EMPTY_LABEL;
        }

        // "<어셈블리> <네임스페이스>.<타입>" 형식이라 마지막 조각만 남긴다.
        int assemblySplit = fullTypename.LastIndexOf(' ');
        string typeName = assemblySplit < 0 ? fullTypename : fullTypename.Substring(assemblySplit + 1);
        int namespaceSplit = typeName.LastIndexOf('.');
        if (0 <= namespaceSplit)
        {
            typeName = typeName.Substring(namespaceSplit + 1);
        }

        return ObjectNames.NicifyVariableName(typeName);
    }
}
