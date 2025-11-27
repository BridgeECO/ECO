using UnityEngine;

namespace ECO
{
    public interface IPlayerController
    {
        public IPlayer Player { get; }
        public bool Create(GameObject sceneRootGO);
        public void ShowPlayer();
        public void Move(Vector2 dir);
        public void Jump();
    }
}