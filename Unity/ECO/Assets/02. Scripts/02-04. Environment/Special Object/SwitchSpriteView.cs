using UnityEngine;
using VInspector;

/// <summary>
/// 스위치의 On/Off 상태에 따라 SpriteRenderer의 스프라이트를 교체한다.
/// EnergySwitch, BossEnergySwitch 공통으로 사용 가능한 뷰 컴포넌트.
/// </summary>
public class SwitchSpriteView : UnityEngine.MonoBehaviour
{
    [Foldout("Sprites")]
    [UnityEngine.SerializeField]
    private UnityEngine.SpriteRenderer _spriteRenderer;

    [UnityEngine.SerializeField]
    private UnityEngine.Sprite _spriteOn;

    [UnityEngine.SerializeField]
    private UnityEngine.Sprite _spriteOff;

    public void Refresh(bool isOn)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        _spriteRenderer.sprite = isOn ? _spriteOn : _spriteOff;
    }
}
