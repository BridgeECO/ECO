using System.Collections.Generic;

/// <summary>
/// ERegions 열거형 값을 UI에 표시할 한국어 지역명으로 변환한다.
/// 새 지역 추가 시 이 클래스의 딕셔너리만 수정하면 된다.
/// </summary>
public static class SaveSlotRegionNameMapper
{
    private static readonly Dictionary<ERegions, string> RegionNames = new Dictionary<ERegions, string>
    {
        { ERegions.Center,                "중앙부" },
        { ERegions.ScrapNestTutorial,     "폐기장" },
        { ERegions.CityOfSlienceTutorial, "침묵의 도시" },
        { ERegions.ScrapNest,             "폐기장" },
        { ERegions.CityOfSlience,         "침묵의 도시" },
        { ERegions.None,                  "빈 슬롯" },
    };

    public static string GetRegionName(ERegions region)
    {
        if (RegionNames.TryGetValue(region, out string name))
        {
            return name;
        }
        return "알 수 없는 지역";
    }
}
