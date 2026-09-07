using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 월드 오브젝트의 스프라이트를 프레임 단위로 갈아 끼운다. NPC 등장·조작·퇴장 연출이 이걸 쓴다.
/// 프로젝트는 이런 연출에 Animator를 쓰지 않고 프레임 교체로 통일한다.
/// </summary>
[Serializable]
[Preserve]
public class CutsceneActorSpriteStep : CutsceneStepBase
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private List<Sprite> _sprites = new List<Sprite>();

    [SerializeField]
    [Min(0.0001f)]
    private float _frameInterval = 0.0666f;

    [SerializeField]
    [Tooltip("체크하면 끝나지 않고 반복합니다. 동시 실행 그룹 안에서만 쓰세요.")]
    private bool _isLoop = false;

    [SerializeField]
    [Min(0f)]
    private float _loopInterval = 0f;

    [SerializeField]
    private bool _isIgnoreTimeScale = false;

    [SerializeField]
    [Tooltip("재생이 끝난 뒤 남길 그림입니다. 비워 두면 마지막 프레임이 그대로 남습니다.")]
    private Sprite _finalSprite;

    private UI_SpriteFrameRunner _runner;

    // [SerializeReference] 역직렬화 경로에서 필드 이니셜라이저가 도는 보장이 없어 지연 생성한다.
    private UI_SpriteFrameRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = new UI_SpriteFrameRunner();
            }

            return _runner;
        }
    }

    public bool IsLoop => _isLoop;

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (_spriteRenderer == null || _sprites.Count == 0)
        {
            return;
        }

        UI_SpriteFrameSettings settings =
            new UI_SpriteFrameSettings(_frameInterval, _isLoop, _loopInterval, _isIgnoreTimeScale);
        await Runner.PlayAsync(_spriteRenderer, _sprites, settings, true, cancellationToken);

        ApplyLastSprite();
    }

    public override void ApplyFinalState(CutsceneContext context)
    {
        Runner.Stop();
        ApplyLastSprite();
    }

    private void ApplyLastSprite()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (_finalSprite != null)
        {
            _spriteRenderer.sprite = _finalSprite;
            return;
        }

        for (int i = _sprites.Count - 1; 0 <= i; i--)
        {
            if (_sprites[i] != null)
            {
                _spriteRenderer.sprite = _sprites[i];
                return;
            }
        }
    }
}
