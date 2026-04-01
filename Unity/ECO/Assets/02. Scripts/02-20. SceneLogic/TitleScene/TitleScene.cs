using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class TitleScene : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _teamLogoImage;

    [SerializeField]
    private Image _backgroundImage;

    private void Start()
    {
        PlayIntroSequence().Forget();
    }

    private async UniTaskVoid PlayIntroSequence()
    {
        Color logoColor = _teamLogoImage.color;
        logoColor.a = 0f;
        _teamLogoImage.color = logoColor;

        Color bgColor = _backgroundImage.color;
        bgColor.a = 1f;
        _backgroundImage.color = bgColor;

        _teamLogoImage.Fade(1f, 0.5f);
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));

        await UniTask.Delay(System.TimeSpan.FromSeconds(2f));

        _teamLogoImage.Fade(0f, 0.5f);
        await UniTask.Delay(System.TimeSpan.FromSeconds(1f));

        _backgroundImage.Fade(0f, 0.5f);
    }
}
