using System;
using UnityEngine;

[Serializable]
public class TerrainGimmickEntry
{
    [SerializeField]
    private TerrainGimmickBaseSO _gimmickData;

    [SerializeField]
    private Sprite _changeSprite;

    public TerrainGimmickBaseSO GimmickData => _gimmickData;
    public Sprite ChangeSprite => _changeSprite;
}
