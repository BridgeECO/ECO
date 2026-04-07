using UnityEngine;
using VInspector;

public class TerrainObject : MonoBehaviour, IEnergyReceiver
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _visualObject;

    [SerializeField]
    private Collider2D _terrainCollider;

    [SerializeField]
    private SpriteRenderer _targetSpriteRenderer;

    [Foldout("Project")]
    [SerializeField]
    private ETerrainState _terrainState = ETerrainState.Active;

    [SerializeField]
    private bool _hasImageToggleGimmick;

    [SerializeField]
    private bool _hasColliderToggleGimmick;

    [SerializeField]
    private Sprite _activeSprite;

    [SerializeField]
    private Sprite _inactiveSprite;

    private bool _isEnergyActive = false;

    private void Awake()
    {
        RefreshTerrainState();
    }

    public void SetEnergyActive(bool isActive)
    {
        _isEnergyActive = isActive;
        RefreshTerrainState();
    }

    private void RefreshTerrainState()
    {
        bool isActiveState = false;
        switch (_terrainState)
        {
            case ETerrainState.Always:
                isActiveState = true;
                break;
            case ETerrainState.Active:
                isActiveState = _isEnergyActive;
                break;
            case ETerrainState.Inactive:
                isActiveState = !_isEnergyActive;
                break;
        }
        ApplyGimmicks(isActiveState);
    }

    private void ApplyGimmicks(bool isActive)
    {
        if (_hasImageToggleGimmick && _visualObject != null)
        {
            _visualObject.SetActive(isActive);
        }

        if (_hasColliderToggleGimmick && _terrainCollider != null)
        {
            _terrainCollider.enabled = isActive;
        }

        if (_targetSpriteRenderer != null)
        {
            Sprite targetSprite = isActive ? _activeSprite : _inactiveSprite;
            if (targetSprite != null)
            {
                _targetSpriteRenderer.sprite = targetSprite;
            }
        }
    }
}
