using System.Collections.Generic;

/// <summary>
/// ERegions 값을 해당 지역의 씬 이름(ESceneNames)으로 변환한다.
/// 새 지역의 씬이 추가될 때 이 클래스의 딕셔너리만 수정하면 된다.
/// </summary>
public static class RegionSceneMapper
{
    private static readonly Dictionary<ERegions, ESceneNames> RegionToScene = new Dictionary<ERegions, ESceneNames>
    {
        { ERegions.Center,                ESceneNames.CenterRoomScene },
        { ERegions.DumpSiteTutorial,      ESceneNames.Region1Scene },
        { ERegions.CityOfSlienceTutorial, ESceneNames.Region1Scene },
        { ERegions.DumpSite,              ESceneNames.Region1Scene },
        { ERegions.CityOfSlience,         ESceneNames.Region1Scene },
    };

    public static bool TryGetSceneName(ERegions region, out ESceneNames sceneName)
    {
        return RegionToScene.TryGetValue(region, out sceneName);
    }
}
