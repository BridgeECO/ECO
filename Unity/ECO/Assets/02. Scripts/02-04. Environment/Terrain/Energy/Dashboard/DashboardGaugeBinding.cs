using System.Collections.Generic;

/// <summary>
/// 계기판에서 선의 펄스와 막대를 잇는다. 펄스마다 꼬리와 머리를 가장 긴 경로 거리에 대한 비율로 바꿔 막대에 그린다.
/// 코어침도 같은 비율 자리에 있으므로, 막대 끝이 코어침을 지나는 프레임에 코어가 켜진다.
/// </summary>
public class DashboardGaugeBinding
{
    private EnergyLine _line;
    private UI_DashboardGauge _gauge;
    private int _drawnPulseCount = -1;
    private bool _wasMoving;

    public DashboardGaugeBinding(EnergyLine line, UI_DashboardGauge gauge)
    {
        _line = line;
        _gauge = gauge;
    }

    // 막대는 비활성화될 때 구간을 모두 반납하므로, 다시 켜지면 처음부터 다시 그리게 한다.
    public void ResetDrawnState()
    {
        _drawnPulseCount = -1;
        _wasMoving = false;
    }

    // 펄스가 움직이거나 개수가 바뀐 프레임, 그리고 멈춘 직후 한 번만 다시 그린다. 가득 찬 채 멈춘 선은 건드리지 않는다.
    public void Refresh()
    {
        IReadOnlyList<EnergySegment> pulses = _line.Segments;
        float maxDistance = _line.TotalDistance;
        bool isMoving = IsAnyPulseMoving(pulses, maxDistance);
        if (pulses.Count == _drawnPulseCount && !isMoving && !_wasMoving)
        {
            return;
        }

        _wasMoving = isMoving;
        _drawnPulseCount = pulses.Count;
        _gauge.SetPulseCount(pulses.Count);
        if (maxDistance <= 0f)
        {
            return;
        }

        for (int i = 0; i < pulses.Count; i++)
        {
            _gauge.SetPulse(i, pulses[i].TailDistance / maxDistance, pulses[i].HeadDistance / maxDistance);
        }
    }

    private bool IsAnyPulseMoving(IReadOnlyList<EnergySegment> pulses, float maxDistance)
    {
        for (int i = 0; i < pulses.Count; i++)
        {
            if (pulses[i].HeadDistance < maxDistance || pulses[i].IsCuttingOff)
            {
                return true;
            }
        }
        return false;
    }
}
