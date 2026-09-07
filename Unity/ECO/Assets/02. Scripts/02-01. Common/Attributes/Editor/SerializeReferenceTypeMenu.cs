using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// [SerializeReference] 항목의 타입 선택 메뉴. 드로어 본체에서 분리해 파생 드로어들이 함께 쓴다.
/// </summary>
public static class SerializeReferenceTypeMenu
{
    public const string EMPTY_LABEL = "(타입 선택)";

    /// <summary>
    /// baseType을 상속한 구체 타입들을 후보로 띄운다.
    /// isSelectable이 false를 돌려준 타입은 목록에서만 감춘다. 지금 그 타입을 쓰고 있는 항목에서는
    /// 남겨 둬야 무엇이 선택돼 있는지 보인다.
    /// </summary>
    public static void Show(SerializedProperty property, Type baseType,
        Func<Type, bool> isSelectable, Func<string, string> toDisplayName)
    {
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

            if (type != currentType)
            {
                // 은퇴한 타입은 새로 고를 수 없게 감춘다. 클래스를 지우면 저장된 참조가 끊기므로 목록에서만 뺀다.
                // 상속으로 조회하면 은퇴한 타입을 물려받은 새 타입까지 함께 사라진다.
                if (type.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    continue;
                }

                if (isSelectable != null && !isSelectable(type))
                {
                    continue;
                }
            }

            Type selectedType = type;
            menu.AddItem(new GUIContent(toDisplayName(type.Name)), currentType == type,
                () => SetValueType(serializedObject, propertyPath, selectedType));
        }

        menu.ShowAsContext();
    }

    /// <summary>드롭다운 버튼에 적을 이름. 타입을 아직 안 골랐으면 EMPTY_LABEL.</summary>
    public static string GetDisplayName(SerializedProperty property, Func<string, string> toDisplayName)
    {
        string typeName = GetShortTypeName(property);
        return typeName == null ? EMPTY_LABEL : toDisplayName(typeName);
    }

    /// <summary>선택된 타입의 이름만 뽑는다. 타입을 아직 안 골랐으면 null.</summary>
    public static string GetShortTypeName(SerializedProperty property)
    {
        string fullTypename = property.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(fullTypename))
        {
            return null;
        }

        // "<어셈블리> <네임스페이스>.<타입>" 형식이라 어셈블리를 먼저 뗀다.
        // 어셈블리명에도 점이 들어갈 수 있어 순서를 뒤집으면 엉뚱하게 잘린다.
        int assemblySplit = fullTypename.LastIndexOf(' ');
        string typeName = assemblySplit < 0 ? fullTypename : fullTypename.Substring(assemblySplit + 1);

        int namespaceSplit = typeName.LastIndexOf('.');
        return 0 <= namespaceSplit ? typeName.Substring(namespaceSplit + 1) : typeName;
    }

    public static bool HasValue(SerializedProperty property)
    {
        return !string.IsNullOrEmpty(property.managedReferenceFullTypename);
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
}
