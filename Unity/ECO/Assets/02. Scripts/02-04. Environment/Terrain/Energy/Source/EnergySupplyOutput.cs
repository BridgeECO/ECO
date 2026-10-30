using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 에너지원이 정한 공급 여부를 연결된 선과 점등 스프라이트, 효과음에 반영한다.
/// 선이 여러 개여도 효과음은 에너지원의 AudioSource 하나로만 낸다.
/// </summary>
public class EnergySupplyOutput
{
    private IReadOnlyList<EnergyLine> _lines;
    private SwitchSpriteView _spriteView;
    private EnergySupplySfx _supplySfx = new EnergySupplySfx();

    public bool IsSupplying { get; private set; }

    public void Init(IReadOnlyList<EnergyLine> lines, SwitchSpriteView spriteView, AudioSource sfxAudioSource)
    {
        _lines = lines;
        _spriteView = spriteView;
        _supplySfx.Init(lines, sfxAudioSource);
    }

    // 흐름이 선 끝까지 퍼졌는지 매 프레임 보고, 다 퍼졌으면 지속음을 멈춘다.
    public void RefreshSfx()
    {
        _supplySfx.Refresh();
    }

    // 시작할 때와 리셋 직후에는 효과음 없이 상태를 맞춘다.
    // 상시형과 차단형은 흘려보내는 연출 없이 가득 찬 선으로 시작하고, 주기형은 공급 구간부터 새로 흘려보낸다.
    public void SetInitialSupply(bool isSupplying, bool isFlowing)
    {
        IsSupplying = isSupplying;
        _spriteView.Refresh(isSupplying);
        if (!isSupplying)
        {
            return;
        }

        for (int i = 0; i < _lines.Count; i++)
        {
            EnergyLine line = _lines[i];
            if (line == null)
            {
                continue;
            }

            if (isFlowing)
            {
                line.SetSwitchState(true);
                continue;
            }
            line.FillImmediately();
        }
    }

    public void SetSupplying(bool isSupplying, bool canPlaySfx)
    {
        if (IsSupplying == isSupplying)
        {
            return;
        }

        IsSupplying = isSupplying;
        _spriteView.Refresh(isSupplying);
        for (int i = 0; i < _lines.Count; i++)
        {
            if (_lines[i] != null)
            {
                _lines[i].SetSwitchState(isSupplying);
            }
        }

        if (canPlaySfx)
        {
            _supplySfx.Play(isSupplying);
            return;
        }
        _supplySfx.Stop();
    }

    // 리스폰 리셋처럼 흘려보내는 연출과 효과음 없이 선을 즉시 비운다. 점등은 초기 공급을 다시 걸 때 맞춘다.
    public void ClearImmediately()
    {
        IsSupplying = false;
        _supplySfx.Stop();
        for (int i = 0; i < _lines.Count; i++)
        {
            if (_lines[i] != null)
            {
                _lines[i].ResetEnergy();
            }
        }
    }
}
