using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using VInspector;

public class BossEventTrigger : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField]
    private EBoss _targetBossType;
    [SerializeField]
    private float _eventTriggerDistance = 20f;

    private BossBase _targetBoss;
    private PlayerInput _playerInput; //
    private Transform _playerTransform;
    private bool _isEventStarted = false;

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag(nameof(ETags.Player));
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerInput = player.GetComponentInParent<PlayerInput>();
        }
    }

    private void Update()
    {
        if (_isEventStarted || _playerTransform == null)
        {
            return;
        }

        if (_targetBoss == null)
        {
            _targetBoss = BossManager.Instance.GetBoss(_targetBossType);
            if (_targetBoss == null) return;
        }

        float distance = Vector2.Distance(_playerTransform.position, _targetBoss.transform.position);

        if (distance <= _eventTriggerDistance)
        {
            OnCutsceneStarted();
        }
    }

    public void OnCutsceneStarted()
    {
        OnCutsceneFinished();
    }

    public void OnCutsceneFinished()
    {
        this.gameObject.SetActive(false);
    }
}
