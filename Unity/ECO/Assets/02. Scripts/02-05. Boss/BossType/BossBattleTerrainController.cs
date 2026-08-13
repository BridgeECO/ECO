using System.Collections.Generic;

public sealed class BossBattleTerrainController
{
    private readonly IReadOnlyList<FloorData> _floorDatas;
    private readonly BossPathFollower _pathFollower;
    private int _currentFloorIndex;

    public FloorData CurrentFloorData
    {
        get
        {
            if (_floorDatas is null ||
                _currentFloorIndex < 0 ||
                _floorDatas.Count <= _currentFloorIndex)
            {
                return null;
            }

            return _floorDatas[_currentFloorIndex];
        }
    }

    public BossBattleTerrainController(
        IReadOnlyList<FloorData> floorDatas,
        BossPathFollower pathFollower)
    {
        _floorDatas = floorDatas;
        _pathFollower = pathFollower;
    }

    public void Advance()
    {
        _currentFloorIndex++;
        SetCurrentPath();
    }

    public void Reset()
    {
        for (int i = 0; i < _floorDatas.Count; i++)
        {
            FloorData floorData = _floorDatas[i];
            if (floorData != null && floorData.Floor != null)
            {
                floorData.Floor.ResetFloorTransition();
            }
        }

        _currentFloorIndex = 0;
        SetCurrentPath();
    }

    public void SetCurrentPath()
    {
        FloorData floorData = CurrentFloorData;
        if (floorData == null || floorData.ChasingLine == null)
        {
            _pathFollower.Clear();
            return;
        }

        _pathFollower.SetPath(floorData.ChasingLine.GetComputedPath());
    }
}
