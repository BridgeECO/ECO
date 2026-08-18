using System.Collections.Generic;

/// <summary>
/// (대상, 채널)별 원래 값을 한 벌만 보관한다.
/// 리액션마다 따로 들면 남이 바꿔 놓은 뒤의 값을 기준으로 잡아 영영 원래대로 못 돌아간다.
/// </summary>
public class UI_ReactionBaselineStore
{
    private readonly List<UI_ReactionBaseline> _baselines = new List<UI_ReactionBaseline>();

    public bool TryGet(int targetId, EUIReactionChannel channel, out UI_ReactionBaseline baseline)
    {
        for (int i = 0; i < _baselines.Count; i++)
        {
            if (_baselines[i].TargetId == targetId && _baselines[i].Channel == channel)
            {
                baseline = _baselines[i];
                return true;
            }
        }

        baseline = default;
        return false;
    }

    public void Set(in UI_ReactionBaseline baseline)
    {
        for (int i = 0; i < _baselines.Count; i++)
        {
            if (_baselines[i].TargetId == baseline.TargetId && _baselines[i].Channel == baseline.Channel)
            {
                _baselines[i] = baseline;
                return;
            }
        }

        _baselines.Add(baseline);
    }

    /// <summary>
    /// 레이아웃 확정 전에 잡은 잠정값을 버린다. 해당 채널을 아무도 점유하지 않을 때만 호출해야
    /// 재생 중인 값이 기준값으로 굳는 사고를 피할 수 있다.
    /// </summary>
    public void Invalidate(int targetId, EUIReactionChannel channel)
    {
        for (int i = 0; i < _baselines.Count; i++)
        {
            if (_baselines[i].TargetId == targetId && _baselines[i].Channel == channel)
            {
                _baselines.RemoveAt(i);
                return;
            }
        }
    }

    public void Clear()
    {
        _baselines.Clear();
    }
}
