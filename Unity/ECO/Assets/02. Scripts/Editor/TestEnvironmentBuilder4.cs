using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Animations;

public class TestEnvironmentBuilder4
{
    [MenuItem("Tools/Fix Existing Animator Clips (Sprite Swap)")]
    public static void FixExistingClips()
    {
        string[] targetPrefabNames = new string[]
        {
            "P_UI_Arrow_Left",
            "P_UI_Arrow_Right",
            "P_UI_Option_Center",
            "P_UI_Option_Corner",
            "P_UI_SaveItem_Down",
            "P_UI_SaveItem_Up",
            "P_UI_WindowBar_Left",
            "P_UI_WindowBar_Right",
            "P_UI_Popup_PauseMenu",
            "P_UI_Popup_Settings",
            "P_UI_Popup_Settings_CloseConfirm",
            "P_UI_Popup_Settings_ResetConfirm",
            "P_UI_Popup_ExitConfirm"
        };

        string basePath = "Assets/03. Prefabs/03-01. UI";
        string outputFolder = "Assets/03. Prefabs/03-01. UI/AnimatorTests";

        int fixedCount = 0;

        foreach (string prefabName in targetPrefabNames)
        {
            string newPath = $"{outputFolder}/{prefabName}_Animator.prefab";
            GameObject newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
            
            string originalPath = $"{basePath}/{prefabName}.prefab";
            if (!System.IO.File.Exists(originalPath))
            {
                string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
                if (guids.Length > 0)
                    originalPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }
            GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(originalPath);

            if (newPrefab == null || originalPrefab == null) continue;

            Animator[] animators = newPrefab.GetComponentsInChildren<Animator>(true);
            foreach (Animator anim in animators)
            {
                // Find matching object in original prefab to get UI_SpriteAnimator
                string pathInPrefab = AnimationUtility.CalculateTransformPath(anim.transform, newPrefab.transform);
                Transform originalObj = originalPrefab.transform.Find(pathInPrefab);
                if (originalObj == null && pathInPrefab == "") originalObj = originalPrefab.transform;
                
                if (originalObj == null) continue;

                UI_SpriteAnimator sa = originalObj.GetComponent<UI_SpriteAnimator>();
                if (sa == null) continue;

                // Extract properties
                SerializedObject so = new SerializedObject(sa);
                SerializedProperty targetImageProp = so.FindProperty("_targetImage");
                SerializedProperty spritesProp = so.FindProperty("_sprites");
                SerializedProperty frameDelayProp = so.FindProperty("_frameDelay");
                SerializedProperty isLoopProp = so.FindProperty("_isLoop");
                SerializedProperty loopIntervalProp = so.FindProperty("_loopInterval");
                
                Image targetImage = targetImageProp.objectReferenceValue as Image;
                if (targetImage == null) targetImage = sa.GetComponent<Image>();
                
                int frameDelay = frameDelayProp.intValue;
                bool isLoop = isLoopProp.boolValue;
                float loopInterval = loopIntervalProp.floatValue;
                
                int spriteCount = spritesProp.arraySize;
                Sprite[] sprites = new Sprite[spriteCount];
                for (int i = 0; i < spriteCount; i++)
                {
                    sprites[i] = spritesProp.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                }

                // Get Clip from Animator
                AnimatorController controller = anim.runtimeAnimatorController as AnimatorController;
                if (controller == null) continue;

                AnimationClip clip = null;
                foreach (var layer in controller.layers)
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        if (state.state.motion is AnimationClip c)
                        {
                            clip = c;
                            break;
                        }
                    }
                    if (clip != null) break;
                }

                if (clip == null) continue;

                // Clear existing float curves (like scale)
                EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
                foreach (var b in bindings)
                {
                    AnimationUtility.SetEditorCurve(clip, b, null);
                }

                // Add Sprite curve
                if (spriteCount > 0 && targetImage != null)
                {
                    EditorCurveBinding spriteBinding = new EditorCurveBinding();
                    spriteBinding.type = typeof(Image);
                    string targetPath = AnimationUtility.CalculateTransformPath(targetImage.transform, sa.transform);
                    spriteBinding.path = targetPath;
                    spriteBinding.propertyName = "m_Sprite";

                    float fps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60f;
                    float timeStep = frameDelay / fps;

                    List<ObjectReferenceKeyframe> keyFramesList = new List<ObjectReferenceKeyframe>();
                    for (int i = 0; i < spriteCount; i++)
                    {
                        ObjectReferenceKeyframe kf = new ObjectReferenceKeyframe();
                        kf.time = i * timeStep;
                        kf.value = sprites[i];
                        keyFramesList.Add(kf);
                    }

                    if (isLoop && loopInterval > (spriteCount - 1) * timeStep)
                    {
                        ObjectReferenceKeyframe finalKf = new ObjectReferenceKeyframe();
                        finalKf.time = loopInterval;
                        finalKf.value = sprites[spriteCount - 1];
                        keyFramesList.Add(finalKf);
                    }

                    AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFramesList.ToArray());
                    
                    AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.loopTime = isLoop;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);

                    EditorUtility.SetDirty(clip);
                    fixedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Fixed {fixedCount} animation clips to use Sprite swap instead of scale!");
    }
}
