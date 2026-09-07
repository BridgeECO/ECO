using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 튜토리얼 조건 항목에 타입 선택 드롭다운을 그린다.
///
/// 유니티가 [SerializeReference]에 기본 제공하는 타입 선택기는 UI Toolkit 인스펙터에만
/// 붙는다. 이 프로젝트는 VInspector가 IMGUI로 인스펙터를 대체하고 있어 선택기가 따라오지
/// 않고, 리스트에 항목을 추가해도 타입을 고를 방법이 없어 빈 줄만 생긴다. 그래서 직접 그린다.
/// </summary>
[CustomPropertyDrawer(typeof(TutorialConditionBase), true)]
public class TutorialConditionDrawer : PropertyDrawer
{
    private const string EMPTY_LABEL = "(조건 선택)";
    private const string TYPE_NAME_PREFIX = "TC_";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect labelRect = new Rect(headerRect.x, headerRect.y, EditorGUIUtility.labelWidth, headerRect.height);
        Rect dropdownRect = new Rect(headerRect.x + EditorGUIUtility.labelWidth, headerRect.y,
            headerRect.width - EditorGUIUtility.labelWidth, headerRect.height);

        bool hasCondition = HasCondition(property);
        if (hasCondition)
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

        if (hasCondition && property.isExpanded)
        {
            EditorGUI.indentLevel++;
            DrawConditionFields(position, property);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (!HasCondition(property) || !property.isExpanded)
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

    // 자식 필드는 조건 타입이 아니므로 기본 드로어로 그려도 이 드로어로 되돌아오지 않는다.
    private void DrawConditionFields(Rect position, SerializedProperty property)
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
        // 메뉴 콜백은 이 프레임 이후에 실행되므로 SerializedProperty를 그대로 붙들면 무효화된다.
        // 경로와 SerializedObject만 넘겨 그 시점에 다시 찾는다.
        SerializedObject serializedObject = property.serializedObject;
        string propertyPath = property.propertyPath;
        string currentType = property.managedReferenceFullTypename;

        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(currentType),
            () => SetConditionType(serializedObject, propertyPath, null));
        menu.AddSeparator(string.Empty);

        foreach (Type type in TypeCache.GetTypesDerivedFrom<TutorialConditionBase>())
        {
            if (type.IsAbstract || type.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            Type conditionType = type;
            bool isSelected = !string.IsNullOrEmpty(currentType) && currentType.EndsWith(type.Name, StringComparison.Ordinal);
            menu.AddItem(new GUIContent(ToDisplayName(type.Name)), isSelected,
                () => SetConditionType(serializedObject, propertyPath, conditionType));
        }

        menu.ShowAsContext();
    }

    private static void SetConditionType(SerializedObject serializedObject, string propertyPath, Type conditionType)
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.FindProperty(propertyPath);
        if (property == null)
        {
            return;
        }

        // 고른 타입이 지금 것과 같으면 손대지 않는다. 새 인스턴스로 갈아치우면
        // 기획자가 입력해 둔 수치와 참조가 그대로 사라진다.
        if (IsSameType(property, conditionType))
        {
            return;
        }

        property.managedReferenceValue = conditionType == null ? null : Activator.CreateInstance(conditionType);
        property.isExpanded = true;
        serializedObject.ApplyModifiedProperties();
    }

    private static bool IsSameType(SerializedProperty property, Type conditionType)
    {
        if (conditionType == null)
        {
            return !HasCondition(property);
        }

        object currentCondition = property.managedReferenceValue;
        return currentCondition != null && currentCondition.GetType() == conditionType;
    }

    private static bool HasCondition(SerializedProperty property)
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

        return ToDisplayName(typeName);
    }

    // 조건 리스트 안에서는 TC_ 접두사가 군더더기라 라벨에서만 뗀다.
    private static string ToDisplayName(string typeName)
    {
        if (typeName.StartsWith(TYPE_NAME_PREFIX, StringComparison.Ordinal))
        {
            typeName = typeName.Substring(TYPE_NAME_PREFIX.Length);
        }

        return ObjectNames.NicifyVariableName(typeName);
    }
}
