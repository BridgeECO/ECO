using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RegionTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField]
    private ESceneNames _targetSceneName;

    private bool _isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isTriggered)
        {
            return;
        }

        if (other.CompareTag(nameof(ETags.Player)))
        {
            _isTriggered = true;
            SceneTransitionManager.Instance.
            TransitionToNewRegionAsync(_targetSceneName).Forget();
        }
    }



}
