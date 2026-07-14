using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BossCinematicBase : MonoBehaviour
{
    protected CameraController _camController;
    protected CameraEffect _camEffect;

    private void Awake()
    {
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            _camController = mainCamera.GetComponentInParent<CameraController>();
            _camEffect = mainCamera.GetComponent<CameraEffect>();
        }
    }
    public abstract UniTask PlayCinematicAsync(BossBase boss);
}