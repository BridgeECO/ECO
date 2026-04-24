using UnityEngine;
using UnityEngine.Playables;
using VInspector;

public class BossEventTrigger : MonoBehaviour
{
    private PlayerInput _playerInput;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.Player)))
        {
            if (_playerInput = other.GetComponentInParent<PlayerInput>())
            {
                _playerInput.IsDashLocked = true;
            }
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
