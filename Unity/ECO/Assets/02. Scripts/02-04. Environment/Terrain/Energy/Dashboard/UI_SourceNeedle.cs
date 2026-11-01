using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 계기판의 에너지원침. 연결한 선에 에너지원이 공급하는 동안 불이 들어온다.
/// </summary>
public class UI_SourceNeedle : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_ReactorSignalToggle _light = new UI_ReactorSignalToggle();

    private void OnDisable()
    {
        _light.ResetState();
    }

    public void SetLit(bool isLit)
    {
        _light.Set(isLit, this.GetCancellationTokenOnDestroy());
    }
}
