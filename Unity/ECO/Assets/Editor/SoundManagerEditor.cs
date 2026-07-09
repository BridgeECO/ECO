using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SoundManager Inspector를 확장하여 _sfxClips 배열의 각 슬롯 옆에
/// ESfxClip Enum 이름 레이블을 표시한다.
/// 슬롯 수와 Enum 항목 수가 다를 때 경고 박스를 노출한다.
/// </summary>
[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    private SerializedProperty _bgmClipsProp;
    private SerializedProperty _sfxClipsProp;

    private void OnEnable()
    {
        _bgmClipsProp = serializedObject.FindProperty("_bgmClips");
        _sfxClipsProp = serializedObject.FindProperty("_sfxClips");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // VInspector Foldout 영역은 기본 드로어가 처리하도록 위임
        DrawPropertiesExcluding(serializedObject, "_sfxClips", "_bgmClips");

        EditorGUILayout.Space(4f);
        DrawBgmClips();

        EditorGUILayout.Space(4f);
        DrawSfxClipsWithEnumLabels();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBgmClips()
    {
        EditorGUILayout.LabelField("BGM Clips", EditorStyles.boldLabel);

        string[] bgmNames = Enum.GetNames(typeof(EBgmType));
        int bgmCount = bgmNames.Length;

        if (_bgmClipsProp.arraySize != bgmCount)
        {
            EditorGUILayout.HelpBox(
                $"_bgmClips 배열 크기({_bgmClipsProp.arraySize})가 EBgmType 항목 수({bgmCount})와 다릅니다.",
                MessageType.Warning);
        }

        _bgmClipsProp.arraySize = bgmCount;

        for (int i = 0; i < bgmCount; i++)
        {
            SerializedProperty element = _bgmClipsProp.GetArrayElementAtIndex(i);
            string label = i < bgmNames.Length ? bgmNames[i] : $"Element {i}";
            EditorGUILayout.ObjectField(element, typeof(AudioClip), new GUIContent($"  [{i}] {label}"));
        }
    }

    private void DrawSfxClipsWithEnumLabels()
    {
        EditorGUILayout.LabelField("SFX Clips", EditorStyles.boldLabel);

        string[] enumNames = Enum.GetNames(typeof(ESfxClip));
        int enumCount = enumNames.Length;

        if (enumCount == 0)
        {
            EditorGUILayout.HelpBox(
                "ESfxClip에 항목이 없습니다. 음악 파일을 09. SFXs 폴더에 추가 후\n" +
                "Tools > Sound > Generate ESfxClip Enum 을 실행하세요.",
                MessageType.Info);
            return;
        }

        if (_sfxClipsProp.arraySize != enumCount)
        {
            EditorGUILayout.HelpBox(
                $"_sfxClips 배열 크기({_sfxClipsProp.arraySize})가 ESfxClip 항목 수({enumCount})와 다릅니다.\n" +
                "Tools > Sound > Generate ESfxClip Enum 을 재실행 후 배열을 재확인하세요.",
                MessageType.Warning);
        }

        _sfxClipsProp.arraySize = enumCount;

        for (int i = 0; i < enumCount; i++)
        {
            SerializedProperty element = _sfxClipsProp.GetArrayElementAtIndex(i);
            string label = i < enumNames.Length ? enumNames[i] : $"Element {i}";
            EditorGUILayout.ObjectField(element, typeof(AudioClip), new GUIContent($"  [{i}] {label}"));
        }
    }
}
