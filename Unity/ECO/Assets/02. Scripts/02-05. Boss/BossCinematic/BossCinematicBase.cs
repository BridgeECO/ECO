using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BossCinematicBase : MonoBehaviour
{
    public abstract UniTask PlayCinematicAsync(BossBase boss);
}