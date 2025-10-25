using ECO;
using UnityEditor;
using UnityEngine;

namespace ECOEditor
{
    [CustomEditor(typeof(Room))]
    public class RoomInspector : Editor
    {
        private const int c_pixelPerUnit = 24;
        private float _boundaryPiexelWidth = 0;
        private float _boundaryPixelHeight = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var col2D = GetBoxCollider2D();
            if (_boundaryPiexelWidth == 0)
                _boundaryPiexelWidth = col2D.size.x * c_pixelPerUnit;
            if (_boundaryPixelHeight == 0)
                _boundaryPixelHeight = col2D.size.y * c_pixelPerUnit;

            GUILayout.Space(10);
            GUILayout.Label("------여기서부터 에디터 기능-----");
            GUILayout.Space(10);
            GUILayout.Label("카메라 경계 콜라이더 너비(픽셀)");
            _boundaryPiexelWidth = EditorGUILayout.FloatField(_boundaryPiexelWidth);
            GUILayout.Label("카메라 경계 콜라이더 높이(픽셀)");
            _boundaryPixelHeight = EditorGUILayout.FloatField(_boundaryPixelHeight);
            GUILayout.Label($"카메라 경계 콜라이더 너비(실제 유니티 사이즈, 단순 값 ) -> {_boundaryPiexelWidth / c_pixelPerUnit}");
            GUILayout.Label($"카메라 경계 콜라이더 높이(실제 유니티 사이즈, 단순 값 ) -> {_boundaryPixelHeight / c_pixelPerUnit}");

            if (_boundaryPiexelWidth != 0 && _boundaryPixelHeight != 0)
            {
                if (GUILayout.Button("Boundary 설정 하기"))
                {
                    RefreshBoundary();
                }
            }
        }

        private void RefreshBoundary()
        {
            float targetSizeX = _boundaryPiexelWidth / c_pixelPerUnit;
            float targetSizeY = _boundaryPixelHeight / c_pixelPerUnit;

            BoxCollider2D col = GetBoxCollider2D();
            col.isTrigger = true;
            col.size = new Vector2(targetSizeX, targetSizeY);
            col.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        private BoxCollider2D GetBoxCollider2D()
        {
            Room tarRoom = this.target as Room;
            BoxCollider2D col2D;

            if (!UNITY.TryGetComp(out col2D, tarRoom.gameObject, false))
                col2D = tarRoom.gameObject.AddComponent<BoxCollider2D>();

            return col2D;
        }
    }
}