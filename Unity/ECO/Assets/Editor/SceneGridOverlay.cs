using UnityEditor;
using UnityEngine;

public class SceneGridOverlay : EditorWindow
{
    private bool _showGrid = true;
    private float _gridSize = 0.25f;
    private Color _gridColor = Color.black;

    [MenuItem("ECO/Tools/Scene Grid Overlay")]
    public static void ShowWindow()
    {
        GetWindow<SceneGridOverlay>("Grid Overlay");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        _showGrid = EditorGUILayout.Toggle("Show Grid", _showGrid);
        _gridSize = EditorGUILayout.FloatField("Grid Size (Unit)", _gridSize);
        _gridColor = EditorGUILayout.ColorField("Grid Color", _gridColor);

        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_showGrid || _gridSize <= 0f)
        {
            return;
        }

        Handles.color = _gridColor;

        Camera cam = sceneView.camera;
        Vector3 camPos = cam.transform.position;
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        float startX = Mathf.Floor((camPos.x - width / 2f) / _gridSize) * _gridSize;
        float endX = camPos.x + width / 2f;
        float startY = Mathf.Floor((camPos.y - height / 2f) / _gridSize) * _gridSize;
        float endY = camPos.y + height / 2f;

        for (float x = startX; x <= endX; x += _gridSize)
        {
            Handles.DrawLine(new Vector3(x, startY, 0f), new Vector3(x, endY, 0f));
        }

        for (float y = startY; y <= endY; y += _gridSize)
        {
            Handles.DrawLine(new Vector3(startX, y, 0f), new Vector3(endX, y, 0f));
        }
    }
}