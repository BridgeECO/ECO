using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SoundManager 컴포넌트의 Custom Inspector.
/// SoundLibrary 내부의 _bgmClips는 기획 가독성을 위해 각 슬롯 옆에 EBgmType Enum 레이블을 표시하고,
/// 나머지 필드와 SFX 카테고리 리스트들은 기본 및 VInspector 드로우로 출력되도록 구성한다.
/// </summary>
[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    private SerializedProperty _soundLibraryProp;
    private SerializedProperty _bgmClipsProp;

    private void OnEnable()
    {
        _soundLibraryProp = serializedObject.FindProperty("_soundLibrary");
        if (_soundLibraryProp != null)
        {
            _bgmClipsProp = _soundLibraryProp.FindPropertyRelative("_bgmClips");
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. SoundManager의 _soundLibrary 속성을 제외한 모든 필드를 먼저 기본 드로우합니다.
        DrawPropertiesExcluding(serializedObject, "_soundLibrary");

        // 2. SoundLibrary 필드 그리기
        if (_soundLibraryProp != null)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Sound Library (Asset Categories)", EditorStyles.boldLabel);

            // SoundLibrary 내부의 모든 속성 중 _bgmClips만 건너뛰고 기본 드로우합니다.
            SerializedProperty childProp = _soundLibraryProp.Copy();
            SerializedProperty endProp = _soundLibraryProp.GetEndProperty();
            
            bool enterChildren = true;
            while (childProp.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(childProp, endProp))
                {
                    break;
                }
                
                enterChildren = false; // 직계 자식 프로퍼티만 순회

                // _bgmClips는 하단에 별도로 그리므로 제외
                if (childProp.name == "_bgmClips")
                {
                    continue;
                }

                EditorGUILayout.PropertyField(childProp, true);
            }

            // 3. _bgmClips 속성을 Enum 레이블과 함께 드로우합니다.
            if (_bgmClipsProp != null)
            {
                EditorGUILayout.Space(8f);
                DrawBgmClipsWithEnumLabels();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// BGM 에셋 슬롯 옆에 상응하는 EBgmType Enum 값을 레이블로 출력한다.
    /// </summary>
    private void DrawBgmClipsWithEnumLabels()
    {
        EditorGUILayout.LabelField("BGM Clips (Mapped to EBgmType)", EditorStyles.boldLabel);

        string[] bgmNames = Enum.GetNames(typeof(EBgmType));
        int bgmCount = bgmNames.Length;

        // 크기가 Enum 항목 수와 불일치할 경우 수동 경고 출력 및 강제 고정
        if (_bgmClipsProp.arraySize != bgmCount)
        {
            EditorGUILayout.HelpBox(
                $"_bgmClips 리스트 크기({_bgmClipsProp.arraySize})가 EBgmType 항목 수({bgmCount})와 다릅니다. " +
                "자동으로 항목 수에 맞춥니다.",
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
}
