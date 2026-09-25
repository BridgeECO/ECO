using UnityEngine;
using VInspector;

[RequireComponent(typeof(Collider2D))]
public class PlayerDistanceTerrainCheckPoint : MonoBehaviour, IResettable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private BossDistanceTerrainActivation _terrainActivation;

    private bool _hasChecked;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasChecked || !other.CompareTag(nameof(ETags.Player)) || _terrainActivation == null)
        {
            return;
        }

        if (_terrainActivation.TryActivate())
        {
            _hasChecked = true;
        }
    }

    public void ResetState()
    {
        _hasChecked = false;
    }
}
