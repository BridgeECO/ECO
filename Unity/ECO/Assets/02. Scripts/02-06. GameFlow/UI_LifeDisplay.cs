using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_LifeDisplay : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<Image> _lifeIcons;

    [Foldout("Project")]
    [SerializeField]
    private List<Sprite> _idleSprites = new List<Sprite>();

    [SerializeField]
    private List<Sprite> _damagedSprites = new List<Sprite>();

    [Foldout("Settings")]
    [SerializeField]
    private float _idleDuration = 2f;

    [SerializeField]
    private float _damagedDuration = 1f;

    private UI_LifeIconAnimator[] _iconAnimators;
    private int _previousLife;
    private bool _isInitialized;

    #region Unity Lifecycle Methods
    private void Awake()
    {
        InitIconAnimators();
    }

    private void OnEnable()
    {
        InitLifeDisplay();
    }

    // LifeManager와 EventManager가 있는 PersistentScene이 이 UI가 속한 씬보다 늦게 초기화되면
    // OnEnable 시점에는 두 싱글톤이 모두 null이다. 모든 Awake가 끝난 Start에서 한 번 더 시도한다.
    private void Start()
    {
        InitLifeDisplay();
    }

    private void OnDisable()
    {
        StopAllIconAnimations();
        RemoveEventListeners();
    }
    #endregion

    #region Event Handlers
    private void OnLifeChanged()
    {
        int currentLife = LifeManager.Instance.CurrentLife;
        int previousLife = _previousLife;
        _previousLife = currentLife;

        if (currentLife < previousLife)
        {
            PlayLoseLife(currentLife, previousLife);
        }
        else if (previousLife < currentLife)
        {
            PlayGainLife(previousLife, currentLife);
        }
    }
    #endregion

    #region Logic
    private void InitIconAnimators()
    {
        _iconAnimators = new UI_LifeIconAnimator[_lifeIcons.Count];
        for (int i = 0; i < _lifeIcons.Count; i++)
        {
            if (_lifeIcons[i] == null)
            {
                continue;
            }

            _iconAnimators[i] = new UI_LifeIconAnimator(_lifeIcons[i], _idleSprites, _idleDuration,
                _damagedSprites, _damagedDuration);
        }
    }

    // OnEnable과 Start 양쪽에서 호출되므로 중복 등록을 플래그로 막는다.
    private void InitLifeDisplay()
    {
        if (_isInitialized || LifeManager.Instance == null || EventManager.Instance == null)
        {
            return;
        }

        _previousLife = LifeManager.Instance.CurrentLife;
        EventManager.Instance.AddEventListener(EEventType.LifeChanged, OnLifeChanged);
        _isInitialized = true;

        RefreshLifeDisplay();
    }

    private void RefreshLifeDisplay()
    {
        int currentLife = LifeManager.Instance.CurrentLife;
        CancellationToken token = gameObject.GetCancellationTokenOnDestroy();
        for (int i = 0; i < _iconAnimators.Length; i++)
        {
            if (!TryGetIconAnimator(i, out UI_LifeIconAnimator iconAnimator))
            {
                continue;
            }

            if (i < currentLife)
            {
                iconAnimator.Show(token);
            }
            else
            {
                iconAnimator.Hide();
            }
        }
    }

    // 즉사처럼 한 번에 여러 칸을 잃으면 잃은 칸이 모두 함께 깨진다.
    private void PlayLoseLife(int currentLife, int previousLife)
    {
        CancellationToken token = gameObject.GetCancellationTokenOnDestroy();
        for (int i = currentLife; i < previousLife; i++)
        {
            if (TryGetIconAnimator(i, out UI_LifeIconAnimator iconAnimator))
            {
                iconAnimator.PlayLoseAsync(token).Forget();
            }
        }
    }

    // 부활처럼 한 번에 여러 칸이 늘어나면 중간 칸은 즉시 켜고, 맨 위 칸만 연출한다.
    private void PlayGainLife(int previousLife, int currentLife)
    {
        CancellationToken token = gameObject.GetCancellationTokenOnDestroy();
        for (int i = previousLife; i < currentLife - 1; i++)
        {
            if (TryGetIconAnimator(i, out UI_LifeIconAnimator iconAnimator))
            {
                iconAnimator.Show(token);
            }
        }

        if (TryGetIconAnimator(currentLife - 1, out UI_LifeIconAnimator topIconAnimator))
        {
            topIconAnimator.PlayGainAsync(token).Forget();
        }
    }

    private void StopAllIconAnimations()
    {
        for (int i = 0; i < _iconAnimators.Length; i++)
        {
            if (TryGetIconAnimator(i, out UI_LifeIconAnimator iconAnimator))
            {
                iconAnimator.Stop();
            }
        }
    }

    private bool TryGetIconAnimator(int index, out UI_LifeIconAnimator iconAnimator)
    {
        bool isInRange = 0 <= index && index < _iconAnimators.Length;
        iconAnimator = isInRange ? _iconAnimators[index] : null;
        return iconAnimator is not null;
    }

    private void RemoveEventListeners()
    {
        if (!_isInitialized)
        {
            return;
        }

        _isInitialized = false;
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.LifeChanged, OnLifeChanged);
        }
    }
    #endregion
}
