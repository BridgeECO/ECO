using UnityEngine;
using VInspector;

/// <summary>
/// 세이브 포인트의 개화 연출을 재생한다.
/// 사용 전에는 Bloom1의 첫 장으로 멈춰 있다가, 처음 사용되는 순간 Bloom1을 한 번 재생하고 Bloom2를 반복한다.
/// </summary>
public class SavePointAnimator : MonoBehaviour
{
    private readonly int _bloomHash = Animator.StringToHash("Bloom");

    [Foldout("Hierarchy")]
    [SerializeField]
    private SavePoint _savePoint;

    [SerializeField]
    private Animator _animator;

    private void OnEnable()
    {
        _savePoint.OnSaved += PlayBloom;
    }

    private void OnDisable()
    {
        _savePoint.OnSaved -= PlayBloom;
    }

    // SavePoint는 한 번 사용되면 다시 발동하지 않으므로 OnSaved도 세이브 포인트마다 한 번만 온다.
    private void PlayBloom()
    {
        _animator.SetTrigger(_bloomHash);
    }
}
