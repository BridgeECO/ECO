using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI_SpriteAnimator를 UI_Reactor의 Show 신호 항목으로 옮기는 일회성 도구. 이관이 끝나면 이 파일을 지운다.
/// </summary>
public static class UI_SpriteAnimatorMigrator
{
    private const string TOOL_TAG = "[SpriteAnimator 이관]";

    [MenuItem("Tools/UI/UI_SpriteAnimator 이관 조사")]
    private static void Report()
    {
        StringBuilder builder = new StringBuilder();
        int total = 0;
        int blocked = 0;

        foreach (string path in FindPrefabPaths())
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                UI_SpriteAnimator[] animators = root.GetComponentsInChildren<UI_SpriteAnimator>(true);
                for (int i = 0; i < animators.Length; i++)
                {
                    if (!IsOwnedBy(animators[i]))
                    {
                        continue;
                    }

                    total++;
                    if (!AppendLine(builder, path, animators[i]))
                    {
                        blocked++;
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        Debug.Log($"{TOOL_TAG} 컴포넌트 {total}개, 손으로 처리할 것 {blocked}개\n{builder}");
    }

    [MenuItem("Tools/UI/UI_SpriteAnimator 이관 실행")]
    private static void Migrate()
    {
        StringBuilder builder = new StringBuilder();
        int migrated = 0;

        foreach (string path in FindPrefabPaths())
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                migrated += MigratePrefab(builder, path, root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"{TOOL_TAG} {migrated}개 이관 완료\n{builder}");
    }

    private static int MigratePrefab(StringBuilder builder, string path, GameObject root)
    {
        UI_SpriteAnimator[] animators = root.GetComponentsInChildren<UI_SpriteAnimator>(true);
        int migrated = 0;

        for (int i = 0; i < animators.Length; i++)
        {
            if (!IsOwnedBy(animators[i]) || FindSelectableAncestor(animators[i].gameObject) != null)
            {
                continue;
            }

            // 값을 읽어 기록한 뒤 컴포넌트를 지운다. 순서를 바꾸면 대조할 원본이 사라진다.
            AppendLine(builder, path, animators[i]);
            Convert(animators[i]);
            migrated++;
        }

        if (0 < migrated)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }

        return migrated;
    }

    private static void Convert(UI_SpriteAnimator animator)
    {
        GameObject host = animator.gameObject;

        UI_Reactor reactor = host.GetComponent<UI_Reactor>();
        if (reactor == null)
        {
            reactor = host.AddComponent<UI_Reactor>();
        }

        UI_SpriteAnimatorEntryWriter.Write(reactor, animator);
        Object.DestroyImmediate(animator, true);
    }

    /// <summary>
    /// 이 프리팹이 직접 들고 있는 컴포넌트만 true. 중첩 프리팹 인스턴스는 원본 프리팹에서 따로 처리한다.
    /// 걸러내지 않으면 소비처마다 오버라이드로 Reactor를 심어 같은 연출이 여러 벌 생긴다.
    /// </summary>
    private static bool IsOwnedBy(UI_SpriteAnimator animator)
    {
        return !PrefabUtility.IsPartOfPrefabInstance(animator.gameObject);
    }

    /// <summary>도구로 처리 가능하면 true. Selectable 자식이면 Reactor를 여기 둘 수 없어 false.</summary>
    private static bool AppendLine(StringBuilder builder, string path, UI_SpriteAnimator animator)
    {
        GameObject host = animator.gameObject;
        Selectable blocker = FindSelectableAncestor(host);
        Image image = ReadTargetImage(animator);

        builder.Append(blocker == null ? "  [자동] " : "  [수동] ")
            .Append(System.IO.Path.GetFileNameWithoutExtension(path))
            .Append(" / ").Append(host.name)
            .Append(" / 스프라이트 ").Append(ReadSpriteCount(animator))
            .Append("장 / 간격 ").Append(ReadFloat(animator, "_frameInterval"))
            .Append(" / 반복 ").Append(ReadBool(animator, "_isLoop"))
            .Append(image == null ? " / Image 없음" : (image.gameObject == host ? " / Image 같은 오브젝트" : " / Image " + image.name));

        if (blocker != null)
        {
            builder.Append(" / 부모 Selectable: ").Append(blocker.name);
        }

        builder.AppendLine();
        return blocker == null;
    }

    // UI_ReactorValidator와 같은 판정이다. 자기 자신에 Selectable이 있으면 Reactor를 둬도 문제없다.
    private static Selectable FindSelectableAncestor(GameObject host)
    {
        if (host.GetComponent<Selectable>() != null)
        {
            return null;
        }

        return host.GetComponentInParent<Selectable>(true);
    }

    private static IEnumerable<string> FindPrefabPaths()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        for (int i = 0; i < guids.Length; i++)
        {
            yield return AssetDatabase.GUIDToAssetPath(guids[i]);
        }
    }

    private static Image ReadTargetImage(UI_SpriteAnimator animator)
    {
        return new SerializedObject(animator).FindProperty("_targetImage").objectReferenceValue as Image;
    }

    private static int ReadSpriteCount(UI_SpriteAnimator animator)
    {
        return new SerializedObject(animator).FindProperty("_sprites").arraySize;
    }

    private static float ReadFloat(UI_SpriteAnimator animator, string field)
    {
        return new SerializedObject(animator).FindProperty(field).floatValue;
    }

    private static bool ReadBool(UI_SpriteAnimator animator, string field)
    {
        return new SerializedObject(animator).FindProperty(field).boolValue;
    }
}
