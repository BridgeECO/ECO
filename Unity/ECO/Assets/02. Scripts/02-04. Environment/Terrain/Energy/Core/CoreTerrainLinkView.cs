using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 코어와 코어에 등록된 지형을 가느다란 직선으로 잇는다.
/// 선 하나에 [코어, 지형1, 코어, 지형2, ...] 순서로 점을 넣어, 지형이 몇 개든 렌더러 하나로 그린다.
/// </summary>
public class CoreTerrainLinkView : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private EnergyCore _core;

    [SerializeField]
    private LineRenderer _linkRenderer;

    private List<Transform> _terrainTransforms = new List<Transform>();

    // 마지막으로 그린 좌표. 이동형 지형은 매 프레임 움직이므로, 바뀐 점만 렌더러에 다시 넣는다.
    private List<Vector3> _drawnPositions = new List<Vector3>();

    private void Start()
    {
        InitLinks();
    }

    private void LateUpdate()
    {
        RefreshLinks();
    }

    private void InitLinks()
    {
        _terrainTransforms.Clear();
        IReadOnlyList<TerrainObject> terrains = _core.RegisteredTerrains;
        for (int i = 0; i < terrains.Count; i++)
        {
            if (terrains[i] != null)
            {
                _terrainTransforms.Add(terrains[i].transform);
            }
        }

        _drawnPositions.Clear();
        _linkRenderer.positionCount = _terrainTransforms.Count * 2;
        for (int i = 0; i < _linkRenderer.positionCount; i++)
        {
            _drawnPositions.Add(Vector3.zero);
        }
        RefreshLinks();
    }

    private void RefreshLinks()
    {
        Vector3 corePosition = _core.transform.position;
        for (int i = 0; i < _terrainTransforms.Count; i++)
        {
            SetLinkPosition(i * 2, corePosition);
            SetLinkPosition(i * 2 + 1, _terrainTransforms[i].position);
        }
    }

    private void SetLinkPosition(int index, Vector3 position)
    {
        if (_drawnPositions[index] == position)
        {
            return;
        }

        _drawnPositions[index] = position;
        _linkRenderer.SetPosition(index, position);
    }
}
