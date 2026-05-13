using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GridGuide
{
    private static bool _isGridVisible;
    private const float GridSize = 1f;
    private static Color _gridColor = Color.black;
    private const string SHOW_GRID_KEY = "ECO_ShowGrid";
    private const string GRID_COLOR_KEY = "ECO_GridColor";

    static GridGuide()
    {
        _isGridVisible = EditorPrefs.GetBool(SHOW_GRID_KEY, true);

        string colorStr = EditorPrefs.GetString(GRID_COLOR_KEY, ColorUtility.ToHtmlStringRGBA(Color.black));
        if (!ColorUtility.TryParseHtmlString(colorStr, out _gridColor))
        {
            _gridColor = Color.black;
        }

        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("ECO/Grid Guide/Toggle Scene Grid", priority = 1)]
    public static void ToggleGrid()
    {
        _isGridVisible = !_isGridVisible;
        EditorPrefs.SetBool(SHOW_GRID_KEY, _isGridVisible);
        SceneView.RepaintAll();
    }

    [MenuItem("ECO/Grid Guide/Toggle Scene Grid", true)]
    public static bool ToggleGridValidate()
    {
        Menu.SetChecked("ECO/Grid Guide/Toggle Scene Grid", _isGridVisible);
        return true;
    }

    public static Color GridColor
    {
        get => _gridColor;
        set
        {
            _gridColor = value;
            EditorPrefs.SetString(GRID_COLOR_KEY, "#" + ColorUtility.ToHtmlStringRGBA(_gridColor));
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!_isGridVisible) return;

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

public class GridGuideSettingsWindow : EditorWindow
{
    [MenuItem("ECO/Grid Guide/Grid Settings", priority = 2)]
    public static void ShowWindow()
    {
        GetWindow<GridGuideSettingsWindow>("Grid Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        Color newColor = EditorGUILayout.ColorField("Grid Color", GridGuide.GridColor);

        if (EditorGUI.EndChangeCheck())
        {
            GridGuide.GridColor = newColor;
            SceneView.RepaintAll();
        }
    }
}