using Cysharp.Threading.Tasks;
using Org.BouncyCastle.Asn1.Esf;
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
            if (GameSceneManager.Instance != null)
            {
                GameSceneManager.Instance.TransitionToNewRegionAsync(_targetSceneName).Forget();
            }
        }
    }
}
