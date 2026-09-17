using UnityEngine;
using VInspector;

public class BossDistanceTerrainActivation : MonoBehaviour, IResettable
{
    [Foldout("Settings")]
    [SerializeField]
    [Min(0f)]
    private float _activationDistance = 10f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject[] _terrainObjects;

    private BossBase _boss;
    private Transform _playerTransform;
    private bool _hasActivated;

    private void Awake()
    {
        ResetState();
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

        SetTerrainObjectsActive(true);
        _hasActivated = true;
        return true;
    }

    public void ResetState()
    {
        _hasActivated = false;
        SetTerrainObjectsActive(false);
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
