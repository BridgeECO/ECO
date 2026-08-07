using System.Collections.Generic;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Transform _spawnPointForDebug;

    [SerializeField]
    private ERoomType _roomType = ERoomType.Normal;

    private BoxCollider2D _cameraBounds;
    private List<IResettable> _resettables;
    private List<SavePoint> _savePoints;

    public Vector2 MinBounds => _cameraBounds.bounds.min;
    public Vector2 MaxBounds => _cameraBounds.bounds.max;
    public Vector2 Center => _cameraBounds.bounds.center;
    public ERoomType RoomType => _roomType;
    public IReadOnlyList<SavePoint> SavePoints => _savePoints;

    private void Awake()
    {
        _cameraBounds = GetComponent<BoxCollider2D>();
        _resettables = new List<IResettable>();
        GetComponentsInChildren<IResettable>(true, _resettables);
        InitSavePoints();
    }

    public void ResetRoom()
    {
        if (_resettables == null)
        {
            return;
        }

        for (int i = 0; i < _resettables.Count; i++)
        {
            _resettables[i]?.ResetState();
        }
    }

    // 세이브 포인트는 자신이 어느 방에 속하는지 알아야 부활 시 카메라 바운드와 BGM을 복원할 수 있다.
    // 방이 소유권을 주입하는 방식이라 기획자는 Room 하위에 배치하기만 하면 된다.
    private void InitSavePoints()
    {
        _savePoints = new List<SavePoint>();
        GetComponentsInChildren<SavePoint>(true, _savePoints);

        for (int i = 0; i < _savePoints.Count; i++)
        {
            _savePoints[i].InitRoom(this);
        }
    }

#if UNITY_EDITOR
    [Button]
    private void SetThisRoomToCurrentRoom()
    {
        Vector3 spawnPoint = _spawnPointForDebug != null ? _spawnPointForDebug.position : transform.position;
        DebugTool.ChangeCurrentRoom(this, spawnPoint);
    }
#endif
}
