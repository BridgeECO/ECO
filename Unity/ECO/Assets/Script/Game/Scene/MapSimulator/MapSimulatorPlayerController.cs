using UnityEngine;

namespace ECO
{
    public class MapSimulatorPlayerController : IPlayerController
    {
        private MapSimulatorPlayer _player = null;

        public IPlayer Player => _player;

        public bool Create(GameObject sceneRootGO)
        {
            if (!UNITY.TryFindCompWithName(out _player, "c_player", sceneRootGO))
                return false;

            return true;
        }

        public void ShowPlayer()
        {
            _player.Show();
        }

        public void Move(Vector2 dir)
        {
            _player.TF.position += (Vector3)dir * 10f;
        }

        public void Jump()
        {
            throw new System.NotImplementedException();
        }
    }
}