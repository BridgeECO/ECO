using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneGridSystem
{
    private static bool _showGrid;
    private const float GridSize = 1f;
    private static Color _gridColor = Color.black;

    static SceneGridSystem()
    {
        _showGrid = EditorPrefs.GetBool("ECO_ShowGrid", true);

        string colorStr = EditorPrefs.GetString("ECO_GridColor", "#FFFFFF33");
        if (!ColorUtility.TryParseHtmlString(colorStr, out _gridColor))
        {
            _gridColor = Color.black;
        }

        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("ECO/Grid/Toggle Scene Grid", priority = 1)]
    public static void ToggleGrid()
    {
        _showGrid = !_showGrid;
        EditorPrefs.SetBool("ECO_ShowGrid", _showGrid);
        SceneView.RepaintAll();
    }

    [MenuItem("ECO/Grid/Toggle Scene Grid", true)]
    public static bool ToggleGridValidate()
    {
        Menu.SetChecked("ECO/Grid/Toggle Scene Grid", _showGrid);
        return true;
    }

    public static Color GridColor
    {
        get => _gridColor;
        set
        {
            _gridColor = value;
            EditorPrefs.SetString("ECO_GridColor", "#" + ColorUtility.ToHtmlStringRGBA(_gridColor));
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!_showGrid) return;

        Handles.color = _gridColor;

        Camera cam = sceneView.camera;
        Vector3 camPos = cam.transform.position;
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        float startX = Mathf.Floor((camPos.x - width / 2f) / GridSize) * GridSize;
        float endX = camPos.x + width / 2f;
        float startY = Mathf.Floor((camPos.y - height / 2f) / GridSize) * GridSize;
        float endY = camPos.y + height / 2f;

        for (float x = startX; x <= endX; x += GridSize)
        {
            Handles.DrawLine(new Vector3(x, startY, 0f), new Vector3(x, endY, 0f));
        }

        for (float y = startY; y <= endY; y += GridSize)
        {
            Handles.DrawLine(new Vector3(startX, y, 0f), new Vector3(endX, y, 0f));
        }
    }
}

public class SceneGridSettingsWindow : EditorWindow
{
    [MenuItem("ECO/Grid/Grid Settings", priority = 2)]
    public static void ShowWindow()
    {
        GetWindow<SceneGridSettingsWindow>("Grid Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        Color newColor = EditorGUILayout.ColorField("Grid Color", SceneGridSystem.GridColor);

        if (EditorGUI.EndChangeCheck())
        {
            SceneGridSystem.GridColor = newColor;
            SceneView.RepaintAll();
        }
    }
}