using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 계기판의 코어침. 가운데에서 퍼지는 빛과 테두리를 리액터 신호로 켜고 끈다.
/// 빛의 크기는 EnergyDashboard가 정한 양에 따라 가운데 크기에서 가장자리까지 늘어난다.
/// </summary>
public class UI_CoreNeedle : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Tooltip("가운데에서 가장자리로 퍼지는 빛입니다. 크기를 바꿔 차오른 정도를 나타냅니다.")]
    [SerializeField]
    private RectTransform _glowRect;

    [SerializeField]
    private UI_ReactorSignalToggle _glowLight = new UI_ReactorSignalToggle();

    [SerializeField]
    private UI_ReactorSignalToggle _rimLight = new UI_ReactorSignalToggle();

    [Foldout("Settings")]
    [Tooltip("빛이 가장 작을 때의 크기입니다(가장자리까지 1). 이동형 코어침은 지형이 스냅 범위 밖에 있으면 이 크기로 가운데만 밝힙니다.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _centerGlowScale = 0.35f;

    private float _glowAmount = -1f;

    private void OnDisable()
    {
        _glowLight.ResetState();
        _rimLight.ResetState();
        _glowAmount = -1f;
    }

    public void SetLit(bool isLit)
    {
        _glowLight.Set(isLit, this.GetCancellationTokenOnDestroy());
    }

    // 0이면 가운데만, 1이면 가장자리까지 빛이 찬다.
    public void SetGlowAmount(float amount)
    {
        if (_glowAmount == amount)
        {
            return;
        }

        _glowAmount = amount;
        float scale = Mathf.Lerp(_centerGlowScale, 1f, amount);
        _glowRect.localScale = new Vector3(scale, scale, 1f);
    }

    public void SetDocked(bool isDocked)
    {
        _rimLight.Set(isDocked, this.GetCancellationTokenOnDestroy());
    }
}
