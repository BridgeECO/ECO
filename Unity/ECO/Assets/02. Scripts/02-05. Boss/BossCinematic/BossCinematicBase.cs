using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BossCinematicBase : MonoBehaviour
{
    protected CameraController _camController;
    protected CameraEffect _camEffect;

    private void Awake()
    {
        _camController = Camera.main.GetComponentInParent<CameraController>();
        _camEffect = Camera.main.GetComponent<CameraEffect>();
    }
    public abstract UniTask PlayCinematicAsync(BossBase boss);
}