using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 켜지면 스프라이트 시퀀스를 재생하는 UI 장식용 컴포넌트.
/// </summary>
public class UI_SpriteAnimator : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _targetImage;

    [Foldout("Project")]
    [SerializeField]
    private List<Sprite> _sprites = new List<Sprite>();

    [Foldout("Settings")]
    [SerializeField]
    [FormerlySerializedAs("_frameDelay")]
    private float _frameInterval = 0.0333f;

    [SerializeField]
    private bool _isLoop = true;

    [SerializeField, Range(2f, 10f)]
    private float _loopInterval = 2f;

    private readonly UI_SpriteFrameRunner _runner = new UI_SpriteFrameRunner();

    private CancellationTokenSource _cts;

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _runner.PlayAsync(_targetImage, _sprites, BuildSettings(), true, _cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _runner.Stop();
    }
    #endregion

    #region Logic
    /// <summary>
    /// 프레임을 역순으로 되돌린다. 팝업이 닫힐 때 호출부가 끝까지 기다린 뒤 다음 단계로 넘어간다.
    /// 러너가 한 벌뿐이라 진행 중이던 정재생은 여기서 자동으로 끊긴다.
    /// </summary>
    public UniTask PlayReverseAsync(CancellationToken cancellationToken)
    {
        return _runner.PlayAsync(_targetImage, _sprites, BuildSettings(), false, cancellationToken);
    }

    // 일시정지 메뉴가 timeScale을 0으로 만들기 때문에 UI 연출은 항상 unscaled로 돈다.
    private UI_SpriteFrameSettings BuildSettings()
    {
        return new UI_SpriteFrameSettings(_frameInterval, _isLoop, _loopInterval, true);
    }
    #endregion
}
