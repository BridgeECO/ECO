using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using ECO;            // TempFSM (런타임)
using ECOEditor;     // FSMLayoutData (에디터용)

namespace ECOEditor
{
    public class EditorFSM : EditorWindow
    {
        private const string LAYOUT_ROOT = "Assets/ECO/EditorFSM_Layouts";
        private const double TRANSITION_GLOW_SECONDS = 1.0;
        private const double SCAN_INTERVAL_SECONDS = 0.5;

        private static readonly Vector2 NODE_SIZE = new Vector2(120f, 48f);
        private static readonly Color COLOR_EDGE = new Color(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color COLOR_EDGE_HOVER = new Color(1f, 0.2f, 0.2f, 1f);
        private static readonly Color COLOR_ACTIVE_GLOW = new Color(1f, 0.25f, 0.25f, 0.8f);

        [MenuItem("ECO/Editor FSM")]
        public static void Open()
        {
            EditorFSM window = GetWindow<EditorFSM>("Editor FSM");
            window.Show();
        }

        private TempFSM[] _sceneFsms;
        private TempFSM _selectedFsm;
        private FSMLayoutData _layoutAsset;

        // 안정 복원을 위한 키(InstanceID 대신)
        private string _selectedFsmKey;

        private Vector2 _graphScroll;
        private float _zoom = 1f;

        private string _draggingNode;
        private Vector2 _dragOffset;

        private GUIStyle _nodeStyle;
        private GUIStyle _nodeActiveStyle;
        private GUIStyle _nodeSelectedStyle;

        private (string from, string to) _lastTransition;
        private double _lastTransitionTime;

        private bool _isAutoScan = true;
        private double _lastScanAt;

        private void OnEnable()
        {
            RefreshSceneFsms();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.update -= OnEditorUpdate;

            if (_selectedFsm != null) _selectedFsm.OnStateChanged -= OnRuntimeStateChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            // 플레이 전환 직후에는 Hierarchy가 안정되기까지 한 프레임 정도 필요
            EditorApplication.delayCall += () =>
            {
                RefreshSceneFsms();     // 재스캔
                ReselectByKeyIfPossible(); // 안정키로 재선택
                Repaint();
            };
        }

        private void OnSelectionChanged()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) return;

            TempFSM fsm = go.GetComponent<TempFSM>();
            if (fsm != null) SelectFsm(fsm);
        }

        private void OnHierarchyChange()
        {
            if (_isAutoScan) RefreshSceneFsms();
        }

        private void OnEditorUpdate()
        {
            if (!_isAutoScan) return;
            double now = EditorApplication.timeSinceStartup;
            if (now - _lastScanAt >= SCAN_INTERVAL_SECONDS)
            {
                _lastScanAt = now;
                RefreshSceneFsms();
            }
        }

        // ===== Helpers =====
        private static void EnsureFolder(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string Sanitize(string name)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "_");
            return name;
        }

        private static string GetHierarchyPath(Transform t)
        {
            // 루트까지 올라가며 경로 문자열 생성
            var sb = new StringBuilder(128);
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            while (stack.Count > 0)
            {
                if (sb.Length > 0) sb.Append('/');
                sb.Append(stack.Pop());
            }
            return sb.ToString();
        }

        private static string GetStableKey(TempFSM fsm)
        {
            if (fsm == null) return null;
            string scene = fsm.gameObject.scene.IsValid() ? fsm.gameObject.scene.name : "NoScene";
            string path = GetHierarchyPath(fsm.transform);
            return $"{scene}|{path}";
        }

        private void CreateStylesIfNeeded()
        {
            if (_nodeStyle != null) return;

            _nodeStyle = new GUIStyle("Box")
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Normal,
                fontSize = 12,
                padding = new RectOffset(6, 6, 6, 6)
            };

            _nodeSelectedStyle = new GUIStyle(_nodeStyle);
            _nodeSelectedStyle.normal.textColor = Color.white;
            _nodeSelectedStyle.normal.background = Texture2D.grayTexture;

            _nodeActiveStyle = new GUIStyle(_nodeStyle);
            _nodeActiveStyle.normal.textColor = Color.white;
            _nodeActiveStyle.fontStyle = FontStyle.Bold;
        }

        private void RefreshSceneFsms()
        {
            TempFSM[] all = Resources.FindObjectsOfTypeAll<TempFSM>();
            var list = new List<TempFSM>(all.Length);

            foreach (TempFSM f in all)
            {
                if (f == null) continue;
                if (EditorUtility.IsPersistent(f)) continue;

                GameObject go = f.gameObject;
                if ((go.hideFlags & HideFlags.HideInHierarchy) != 0) continue;
                if (!go.scene.IsValid()) continue;

                list.Add(f);
            }

            _sceneFsms = list.ToArray();

            // 안정키로 기존 선택 복원 시도
            if (!string.IsNullOrEmpty(_selectedFsmKey))
                ReselectByKeyIfPossible();

            // 여전히 선택이 없으면 첫 항목 자동 선택
            if (_selectedFsm == null && _sceneFsms.Length > 0)
                SelectFsm(_sceneFsms[0]);
        }

        private void ReselectByKeyIfPossible()
        {
            if (string.IsNullOrEmpty(_selectedFsmKey) || _sceneFsms == null) return;

            for (int i = 0; i < _sceneFsms.Length; i++)
            {
                if (GetStableKey(_sceneFsms[i]) == _selectedFsmKey)
                {
                    if (_selectedFsm != _sceneFsms[i]) SelectFsm(_sceneFsms[i]);
                    return;
                }
            }

            // 키에 해당하는 오브젝트가 사라졌다면 선택 해제
            _selectedFsm = null;
        }

        private void SelectFsm(TempFSM fsm)
        {
            if (_selectedFsm == fsm) return;

            if (_selectedFsm != null) _selectedFsm.OnStateChanged -= OnRuntimeStateChanged;

            _selectedFsm = fsm;
            _selectedFsmKey = GetStableKey(fsm);

            EnsureLayoutAsset();

            if (_selectedFsm != null) _selectedFsm.OnStateChanged += OnRuntimeStateChanged;

            Repaint();
        }

        private void OnRuntimeStateChanged(string from, string to)
        {
            _lastTransition = (from, to);
            _lastTransitionTime = EditorApplication.timeSinceStartup;
            Repaint();
        }

        // 선택/전환 직후에도 그래프가 항상 준비되도록
        private void ValidateAndEnsureLayout()
        {
            if (_selectedFsm == null) { _layoutAsset = null; return; }
            if (_layoutAsset == null || _layoutAsset.fsmOwner != _selectedFsm.gameObject)
                EnsureLayoutAsset();
            EnsureNodePositions();
        }

        private void EnsureNodePositions()
        {
            if (_selectedFsm == null || _layoutAsset == null) return;

            var names = new List<string>();
            foreach (var s in _selectedFsm.states) names.Add(s.name);
            if (names.Count == 0) return;

            Vector2 center = new Vector2(400f, 300f);
            float radius = 200f;
            int count = names.Count;
            for (int i = 0; i < count; i++)
            {
                string name = names[i];
                if (!_layoutAsset.TryGet(name, out _))
                {
                    float angle = i / (float)count * Mathf.PI * 2f;
                    Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    _layoutAsset.Set(name, pos);
                }
            }
        }

        private void EnsureLayoutAsset()
        {
            if (_selectedFsm == null) { _layoutAsset = null; return; }

            EnsureFolder(LAYOUT_ROOT);

            string scene = _selectedFsm.gameObject.scene.IsValid() ? _selectedFsm.gameObject.scene.name : "NoScene";
            string pathKey = GetHierarchyPath(_selectedFsm.transform);
            string safeScene = Sanitize(scene);
            string safeKey = Sanitize(pathKey);

            // 안정 키 기반 파일명 (InstanceID 사용하지 않음)
            string fileName = $"{safeScene}_{safeKey}.asset";
            string path = $"{LAYOUT_ROOT}/{fileName}";

            _layoutAsset = AssetDatabase.LoadAssetAtPath<FSMLayoutData>(path);
            if (_layoutAsset == null)
            {
                _layoutAsset = ScriptableObject.CreateInstance<FSMLayoutData>();
                _layoutAsset.fsmOwner = _selectedFsm.gameObject;

                // 초기 원형 배치
                var names = new List<string>();
                foreach (var s in _selectedFsm.states) names.Add(s.name);

                Vector2 center = new Vector2(400f, 300f);
                float radius = 200f;
                int count = Mathf.Max(1, names.Count);
                for (int i = 0; i < names.Count; i++)
                {
                    float angle = i / (float)count * Mathf.PI * 2f;
                    Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    _layoutAsset.Set(names[i], pos);
                }

                string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(_layoutAsset, uniquePath);
                AssetDatabase.SaveAssets();
            }
        }

        // ===== GUI =====
        private void OnGUI()
        {
            CreateStylesIfNeeded();
            ValidateAndEnsureLayout();

            DrawToolbar();
            EditorGUILayout.Space(3);

            if ((_sceneFsms == null || _sceneFsms.Length == 0) && Event.current.type == EventType.Layout)
                RefreshSceneFsms();

            if (_selectedFsm == null)
            {
                EditorGUILayout.HelpBox("씬에서 TempFSM을 가진 오브젝트를 선택하세요.", MessageType.Info);
                return;
            }

            if (_selectedFsm.states == null || _selectedFsm.states.Count == 0)
            {
                EditorGUILayout.HelpBox("이 FSM에는 상태가 없습니다. TempFSM.states에 Idle/Run/Jump/Fall 등을 추가하세요.", MessageType.Warning);
                return;
            }

            using (var sv = new EditorGUILayout.ScrollViewScope(_graphScroll))
            {
                _graphScroll = sv.scrollPosition;

                Rect canvasRect = GUILayoutUtility.GetRect(2000, 2000);
                DrawGrid(canvasRect, 20f, new Color(0, 0, 0, 0.08f));
                DrawGrid(canvasRect, 100f, new Color(0, 0, 0, 0.12f));

                Handles.BeginGUI();
                DrawTransitions();
                Handles.EndGUI();

                DrawNodes();
                ProcessNodeEvents();
            }

            EditorGUILayout.Space(2);
            string state = _selectedFsm.CurrentState;
            EditorGUILayout.LabelField($"현재 상태: {(string.IsNullOrEmpty(state) ? "(없음)" : state)}");
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                string[] options = BuildFsmOptions();
                int currentIndex = IndexOfSelected();
                int nextIndex = EditorGUILayout.Popup(currentIndex, options, EditorStyles.toolbarPopup, GUILayout.Width(360));
                if (nextIndex != currentIndex)
                {
                    if (_sceneFsms != null && _sceneFsms.Length > 0 && nextIndex >= 0 && nextIndex < _sceneFsms.Length)
                        SelectFsm(_sceneFsms[nextIndex]);
                }

                GUILayout.Space(8);

                bool nextAuto = GUILayout.Toggle(_isAutoScan, "Auto-Scan", EditorStyles.toolbarButton, GUILayout.Width(80));
                if (nextAuto != _isAutoScan) _isAutoScan = nextAuto;

                if (GUILayout.Button("Scan Now", EditorStyles.toolbarButton, GUILayout.Width(90)))
                {
                    RefreshSceneFsms();
                    ReselectByKeyIfPossible();
                    Repaint();
                }

                if (GUILayout.Button("Rebuild Layout", EditorStyles.toolbarButton, GUILayout.Width(110)))
                {
                    _layoutAsset = null;
                    EnsureLayoutAsset();
                    EnsureNodePositions();
                    Repaint();
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label("Zoom", GUILayout.Width(36));
                _zoom = GUILayout.HorizontalSlider(_zoom, 0.5f, 2.0f, GUILayout.Width(120));

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    RefreshSceneFsms();
                    ReselectByKeyIfPossible();
                    EnsureLayoutAsset();
                    Repaint();
                }

                if (GUILayout.Button("Save Layout", EditorStyles.toolbarButton, GUILayout.Width(90)))
                {
                    if (_layoutAsset != null) EditorUtility.SetDirty(_layoutAsset);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private string[] BuildFsmOptions()
        {
            if (_sceneFsms == null) _sceneFsms = Array.Empty<TempFSM>();
            var list = new List<string>(_sceneFsms.Length);
            foreach (TempFSM f in _sceneFsms)
            {
                string label = f != null ? $"{f.gameObject.name}  ({f.gameObject.scene.name})" : "(null)";
                list.Add(label);
            }
            return list.ToArray();
        }

        private int IndexOfSelected()
        {
            if (_selectedFsm == null || _sceneFsms == null || _sceneFsms.Length == 0) return 0;
            for (int i = 0; i < _sceneFsms.Length; i++)
                if (_sceneFsms[i] == _selectedFsm) return i;
            return 0;
        }

        private void DrawGrid(Rect rect, float gridSpacing, Color gridColor)
        {
            int cols = Mathf.CeilToInt(rect.width / gridSpacing);
            int rows = Mathf.CeilToInt(rect.height / gridSpacing);
            Handles.color = gridColor;

            for (int i = 0; i < cols; i++)
                Handles.DrawLine(new Vector2(rect.x + i * gridSpacing, rect.y),
                                 new Vector2(rect.x + i * gridSpacing, rect.yMax));

            for (int j = 0; j < rows; j++)
                Handles.DrawLine(new Vector2(rect.x, rect.y + j * gridSpacing),
                                 new Vector2(rect.xMax, rect.y + j * gridSpacing));
        }

        private void DrawNodes()
        {
            if (_selectedFsm == null || _layoutAsset == null) return;

            foreach (var s in _selectedFsm.states)
            {
                bool hasPos = _layoutAsset.TryGet(s.name, out Vector2 pos);
                if (!hasPos) _layoutAsset.Set(s.name, Vector2.zero);

                Rect rect = GetNodeRect(pos);
                bool isActive = (_selectedFsm.CurrentState == s.name);

                if (isActive)
                {
                    Rect glowRect = rect;
                    glowRect.xMin -= 6f; glowRect.yMin -= 6f;
                    glowRect.xMax += 6f; glowRect.yMax += 6f;
                    EditorGUI.DrawRect(glowRect, COLOR_ACTIVE_GLOW);
                }

                GUIStyle style = isActive ? _nodeActiveStyle : _nodeStyle;
                if (_draggingNode == s.name) style = _nodeSelectedStyle;

                GUILayout.BeginArea(rect);
                GUILayout.Label(s.name, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                GUILayout.EndArea();

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                {
                    _selectedFsm.SetState(s.name);
                    _draggingNode = s.name;
                    _dragOffset = Event.current.mousePosition - pos;
                    GUI.FocusControl(null);
                    Event.current.Use();
                }
            }
        }

        private void ProcessNodeEvents()
        {
            if (_draggingNode != null && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                Vector2 newPos = Event.current.mousePosition - _dragOffset;
                _layoutAsset.Set(_draggingNode, newPos);
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0) _draggingNode = null;
        }

        private Rect GetNodeRect(Vector2 pos)
        {
            Vector2 size = NODE_SIZE * _zoom;
            return new Rect(pos.x, pos.y, size.x, size.y);
        }

        private Vector2 GetNodeCenter(string stateName)
        {
            bool has = _layoutAsset.TryGet(stateName, out Vector2 pos);
            if (!has) pos = Vector2.zero;
            Rect rect = GetNodeRect(pos);
            return rect.center;
        }

        private void DrawTransitions()
        {
            if (_selectedFsm == null) return;

            double now = EditorApplication.timeSinceStartup;
            foreach (var t in _selectedFsm.transitions)
            {
                Vector2 from = GetNodeCenter(t.from);
                Vector2 to = GetNodeCenter(t.to);

                bool isRecent = (t.from == _lastTransition.from && t.to == _lastTransition.to && (now - _lastTransitionTime) <= TRANSITION_GLOW_SECONDS);

                Color col = isRecent ? COLOR_EDGE_HOVER : COLOR_EDGE;
                float thickness = isRecent ? 3.5f : 2.0f;

                Vector2 dir = (to - from);
                Vector2 perp = new Vector2(-dir.y, dir.x).normalized;
                float dist = dir.magnitude;
                float offset = Mathf.Min(120f, dist * 0.5f);

                Vector2 p0 = from;
                Vector2 p1 = from + dir * 0.2f + perp * 0.1f * offset;
                Vector2 p2 = to - dir * 0.2f - perp * 0.1f * offset;
                Vector2 p3 = to;

                Handles.DrawBezier(p0, p3, p1, p2, col, null, thickness);
                DrawArrowHead(p3, (p3 - p2).normalized, col, 10f + (isRecent ? 2f : 0f));
            }
        }

        private void DrawArrowHead(Vector2 tip, Vector2 dir, Color col, float size)
        {
            Vector2 right = Quaternion.Euler(0, 0, 20) * (-dir);
            Vector2 left = Quaternion.Euler(0, 0, -20) * (-dir);

            Handles.color = col;
            Handles.DrawAAConvexPolygon(
                tip,
                tip + right * size,
                tip + left * size
            );
        }
    }
}
