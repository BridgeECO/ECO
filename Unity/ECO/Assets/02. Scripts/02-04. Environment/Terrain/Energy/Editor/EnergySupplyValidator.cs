using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 에너지원과 스위치의 연결을 검사한다. 기획서는 선 하나에 에너지원 하나, 에너지원 하나에 스위치 하나를 전제한다.
/// </summary>
public static class EnergySupplyValidator
{
    public static int Report(EnergyLine[] lines, EnergySource[] sources, EnergySwitch[] switches)
    {
        Dictionary<EnergyLine, int> lineDrivers = new Dictionary<EnergyLine, int>();
        Dictionary<EnergySource, int> sourceSwitchCounts = new Dictionary<EnergySource, int>();
        int invalidCount = 0;

        for (int i = 0; i < sources.Length; i++)
        {
            invalidCount += CountSourceLines(sources[i], lineDrivers);
        }
        for (int i = 0; i < switches.Length; i++)
        {
            invalidCount += CountSwitchTargets(switches[i], lineDrivers, sourceSwitchCounts);
        }

        invalidCount += ReportLineDrivers(lines, lineDrivers);
        invalidCount += ReportSourceSwitches(sources, sourceSwitchCounts);
        return invalidCount;
    }

    private static int CountSourceLines(EnergySource source, Dictionary<EnergyLine, int> lineDrivers)
    {
        IReadOnlyList<EnergyLine> lines = source.ConnectedLines;
        int validCount = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] == null)
            {
                continue;
            }
            validCount++;
            lineDrivers[lines[i]] = lineDrivers.TryGetValue(lines[i], out int count) ? count + 1 : 1;
        }

        if (0 < validCount)
        {
            return 0;
        }
        Debug.LogWarning($"[Energy] 에너지원 '{source.name}'에 연결된 에너지선이 없습니다.", source);
        return 1;
    }

    private static int CountSwitchTargets(EnergySwitch energySwitch, Dictionary<EnergyLine, int> lineDrivers,
        Dictionary<EnergySource, int> sourceSwitchCounts)
    {
        int targetCount = 0;
        IReadOnlyList<EnergySource> sources = energySwitch.TargetSources;
        for (int i = 0; i < sources.Count; i++)
        {
            if (sources[i] == null)
            {
                continue;
            }
            targetCount++;
            sourceSwitchCounts[sources[i]] = sourceSwitchCounts.TryGetValue(sources[i], out int count) ? count + 1 : 1;
        }

        IReadOnlyList<EnergyLine> legacyLines = energySwitch.LegacyConnectedLines;
        for (int i = 0; i < legacyLines.Count; i++)
        {
            if (legacyLines[i] == null)
            {
                continue;
            }
            targetCount++;
            lineDrivers[legacyLines[i]] = lineDrivers.TryGetValue(legacyLines[i], out int count) ? count + 1 : 1;
        }

        if (0 < targetCount)
        {
            return 0;
        }
        Debug.LogWarning($"[Energy] 스위치 '{energySwitch.name}'이(가) 제어하는 에너지원이 없습니다.", energySwitch);
        return 1;
    }

    private static int ReportLineDrivers(EnergyLine[] lines, Dictionary<EnergyLine, int> lineDrivers)
    {
        int invalidCount = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            lineDrivers.TryGetValue(lines[i], out int driverCount);
            if (driverCount == 1)
            {
                continue;
            }

            invalidCount++;
            string message = driverCount == 0
                ? $"[Energy] 에너지선 '{lines[i].name}'에 에너지를 공급하는 에너지원이 없습니다. 보스 컷신처럼 코드로 켜는 선이 아니라면 에너지원을 연결하세요."
                : $"[Energy] 에너지선 '{lines[i].name}'을(를) 공급하는 쪽이 {driverCount}곳입니다. 에너지선 하나에는 에너지원 하나만 연결하세요.";
            Debug.LogWarning(message, lines[i]);
        }
        return invalidCount;
    }

    private static int ReportSourceSwitches(EnergySource[] sources, Dictionary<EnergySource, int> sourceSwitchCounts)
    {
        int invalidCount = 0;
        for (int i = 0; i < sources.Length; i++)
        {
            sourceSwitchCounts.TryGetValue(sources[i], out int switchCount);
            bool isSwitchPattern = sources[i].SupplyPattern == EEnergySupplyPattern.Switch;

            if (isSwitchPattern && switchCount == 0)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 스위치형 에너지원 '{sources[i].name}'을(를) 작동시키는 스위치가 없습니다.", sources[i]);
            }
            else if (1 < switchCount)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 에너지원 '{sources[i].name}'에 스위치 {switchCount}개가 연결되어 있습니다. 하나라도 작동 중이면 작동한 것으로 처리합니다.", sources[i]);
            }
        }
        return invalidCount;
    }
}
