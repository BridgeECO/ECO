using UnityEngine;
using VInspector;

public class BossDistanceTerrainActivation : MonoBehaviour, IResettable
{
    [Foldout("Settings")]
    [SerializeField]
    [Min(0f)]
    private float _activationDistance = 10f;

    [Header("Drop Motion")]
    [SerializeField]
    [Min(0f)]
    [Tooltip("씬에 배치한 착지 위치보다 위에서 시작하는 높이입니다.")]
    private float _dropHeight = 8f;

    [SerializeField]
    [Min(0f)]
    [Tooltip("착지 위치까지 이동하는 시간입니다. 0이면 즉시 배치됩니다.")]
    private float _dropDuration = 0.6f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject[] _terrainObjects;

    private BossBase _boss;
    private Transform _playerTransform;
    private bool _hasActivated;
    private BossTerrainDropMotion _dropMotion;

    private void Awake()
    {
        InitDropMotion();
        ResetState();
    }

    private void Update()
    {
        if (_dropMotion is null)
        {
            return;
        }

        _dropMotion.Tick(Time.deltaTime);
    }

    private void OnDisable()
    {
        if (_dropMotion is null)
        {
            return;
        }

        _dropMotion.Complete();
    }

    public void InitBattleContext(BossBase boss, Transform playerTransform)
    {
        _boss = boss;
        _playerTransform = playerTransform;
    }

    public bool TryActivate()
    {
        if (_boss == null || _playerTransform == null)
        {
            return false;
        }

        if (_hasActivated)
        {
            return true;
        }

        Vector2 distanceOffset = _boss.transform.position - _playerTransform.position;
        float activationDistanceSqr = _activationDistance * _activationDistance;

        if (distanceOffset.sqrMagnitude <= activationDistanceSqr)
        {
            return true;
        }

        _hasActivated = true;
        _dropMotion.Play(_dropHeight, _dropDuration);
        return true;
    }

    public void ResetState()
    {
        _hasActivated = false;
        InitDropMotion();
        _dropMotion.Complete();
        SetTerrainObjectsActive(false);
    }

    private void InitDropMotion()
    {
        if (_dropMotion is not null)
        {
            return;
        }

        _dropMotion = new BossTerrainDropMotion(_terrainObjects);
    }

    private void SetTerrainObjectsActive(bool isActive)
    {
        if (_terrainObjects == null)
        {
            return;
        }

        for (int i = 0; i < _terrainObjects.Length; i++)
        {
            GameObject terrainObject = _terrainObjects[i];

            if (terrainObject == null)
            {
                continue;
            }

            terrainObject.SetActive(isActive);
        }
    }
}
