using Cysharp.Threading.Tasks;
using UnityEngine;

// 1. 화면 페이드 아웃
// 2. 기존 씬 언로드
// 3. 새 씬 로드
// 4. 플레이어 좌표 조정
// 5. 화면 페이드 인
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
