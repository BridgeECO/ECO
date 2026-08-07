using System;
using UnityEngine;

/// <summary>
/// 스위치의 On/Off 상태에 따라 SpriteRenderer의 스프라이트를 교체한다.
/// Unity 생명주기가 필요 없어 컴포넌트가 아닌 직렬화 클래스로 두고,
/// EnergySwitch/BossEnergySwitch가 인라인 필드로 소유한다.
/// </summary>
[Serializable]
public class SwitchSpriteView
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Sprite _spriteOn;

    [SerializeField]
    private Sprite _spriteOff;

    public void Refresh(bool isOn)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        _spriteRenderer.sprite = isOn ? _spriteOn : _spriteOff;
    }
}
