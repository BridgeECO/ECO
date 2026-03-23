using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ECOEditor
{
    [InitializeOnLoad]
    public class EditorSceneDropDownOverlay : EditorToolbarDropdown
    {
        private static bool _isOverlayAdded = false;

        static EditorSceneDropDownOverlay()
        {
            // SceneView 생성 시마다 Overlay 추가
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (_isOverlayAdded)
                return;

            var root = sceneView.rootVisualElement;
            var dropdown = MakeDropdownBtn();
            FillSceneMenu(dropdown);

            root.Add(dropdown);
            _isOverlayAdded = true;
        }

        private static ToolbarMenu MakeDropdownBtn()
        {
            var dropdown = new ToolbarMenu();
            dropdown.text = "Scenes";
            dropdown.style.position = Position.Absolute;

            // 위치 (우측 상단처럼 보이도록)
            dropdown.style.top = 4;
            dropdown.style.right = 4;

            dropdown.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f));
            dropdown.style.borderTopLeftRadius = 3;
            dropdown.style.borderTopRightRadius = 3;
            dropdown.style.borderBottomLeftRadius = 3;
            dropdown.style.borderBottomRightRadius = 3;
            dropdown.style.paddingLeft = 6;
            dropdown.style.paddingRight = 6;
            return dropdown;
        }

        private static void FillSceneMenu(ToolbarMenu menu)
        {
            const string SCENE_PATH = "Assets/01. Scene";

            menu.menu.MenuItems().Clear();

            var guids = AssetDatabase.FindAssets("t:Scene", new[] { SCENE_PATH });

            if (guids.Length == 0)
            {
                menu.menu.AppendAction("No scenes found", a => { }, DropdownMenuAction.Status.Disabled);
                return;
            }

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(path);

                menu.menu.AppendAction(name, a =>
                {
                    LoadAndPlay(path);
                });
            }
        }

        private static void LoadAndPlay(string path)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Stop play mode first.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
                EditorApplication.isPlaying = true;
            }
        }
    }
}