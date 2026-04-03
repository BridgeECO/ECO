using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RegionTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string _targetSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Player)))
        {
            GameSceneManager.Instance.TransitionToNewRegionAsync(_targetSceneName).Forget();
        }
    }
}
