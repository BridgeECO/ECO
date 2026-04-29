using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainGimmickEntry
{
    [SerializeField]
    private TerrainGimmickBaseSO _gimmickData;

    [SerializeField]
    private Sprite _changeSprite;

    [SerializeField]
    private float _moveSpeed = 3f;

    [SerializeField]
    private List<Transform> _waypoints;

    public TerrainGimmickBaseSO GimmickData => _gimmickData;
    public Sprite ChangeSprite => _changeSprite;
    public float MoveSpeed => _moveSpeed;
    public List<Transform> Waypoints => _waypoints;
}
