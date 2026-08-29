using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI_SpriteAnimator의 직렬화 값을 UI_Reactor의 Show 신호 항목으로 옮겨 적는다. 일회성 이관 도구의 일부다.
/// </summary>
public static class UI_SpriteAnimatorEntryWriter
{
    private const string ENTRY_LABEL = "등장 애니메이션";

    public static void Write(UI_Reactor reactor, UI_SpriteAnimator animator)
    {
        SerializedObject source = new SerializedObject(animator);
        SerializedObject target = new SerializedObject(reactor);

        // 컴포넌트가 OnEnable에서 재생하던 시점을 그대로 옮긴다.
        target.FindProperty("_isPlayShowOnEnable").boolValue = true;

        SerializedProperty entries = target.FindProperty("_entries");
        int index = entries.arraySize;
        entries.InsertArrayElementAtIndex(index);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);

        WriteTrigger(entry);
        WriteReaction(entry, source, animator);

        target.ApplyModifiedPropertiesWithoutUndo();
    }

    // 리스트에 원소를 추가하면 유니티가 C# 초기값을 무시하므로 쓰는 칸을 전부 지정한다.
    private static void WriteTrigger(SerializedProperty entry)
    {
        entry.FindPropertyRelative("_label").stringValue = ENTRY_LABEL;
        entry.FindPropertyRelative("_isMuted").boolValue = false;
        entry.FindPropertyRelative("_kind").enumValueIndex = (int)EUIReactionTriggerKind.Signal;
        entry.FindPropertyRelative("_signalTrigger").enumValueIndex = (int)EUIReactionSignal.Show;
        entry.FindPropertyRelative("_interruptPolicy").enumValueIndex = (int)EUIReactionInterruptPolicy.Restart;

        // 팝업이 닫힐 때 UI_Popup이 이 항목을 되감는다. 기본값으로 두면 역재생 없이 즉시 복원된다.
        entry.FindPropertyRelative("_exitPolicy").enumValueIndex = (int)EUIReactionExitPolicy.Reverse;
    }

    private static void WriteReaction(SerializedProperty entry, SerializedObject source, UI_SpriteAnimator animator)
    {
        SerializedProperty reactions = entry.FindPropertyRelative("_reactions");
        reactions.ClearArray();
        reactions.InsertArrayElementAtIndex(0);

        SerializedProperty element = reactions.GetArrayElementAtIndex(0);
        element.managedReferenceValue = new UI_SpriteAnimationReaction();

        element.FindPropertyRelative("_target").objectReferenceValue = ResolveTarget(source, animator);
        element.FindPropertyRelative("_frameInterval").floatValue = source.FindProperty("_frameInterval").floatValue;
        element.FindPropertyRelative("_isLoop").boolValue = source.FindProperty("_isLoop").boolValue;
        element.FindPropertyRelative("_loopInterval").floatValue = source.FindProperty("_loopInterval").floatValue;

        // 컴포넌트는 BuildSettings에서 항상 unscaled로 넘겼다. 일시정지 메뉴가 timeScale을 0으로 만든다.
        element.FindPropertyRelative("_isIgnoreTimeScale").boolValue = true;

        CopySprites(source.FindProperty("_sprites"), element.FindPropertyRelative("_sprites"));
    }

    /// <summary>대상 Image가 Reactor와 같은 오브젝트면 비워 둔다. 리액션이 알아서 자기 Image를 찾는다.</summary>
    private static GameObject ResolveTarget(SerializedObject source, UI_SpriteAnimator animator)
    {
        Image image = source.FindProperty("_targetImage").objectReferenceValue as Image;
        if (image == null || image.gameObject == animator.gameObject)
        {
            return null;
        }

        return image.gameObject;
    }

    private static void CopySprites(SerializedProperty source, SerializedProperty target)
    {
        target.ClearArray();
        target.arraySize = source.arraySize;

        for (int i = 0; i < source.arraySize; i++)
        {
            target.GetArrayElementAtIndex(i).objectReferenceValue =
                source.GetArrayElementAtIndex(i).objectReferenceValue;
        }
    }
}
