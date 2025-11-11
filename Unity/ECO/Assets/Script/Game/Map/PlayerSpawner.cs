using UnityEngine;

namespace ECO
{
    public class PlayerSpawner : MonoBase
    {
        [SerializeField] private GameObject _playerPrefab; // Player 프리팹 (필수), 임시로 Serialize
        private string _playerRootName = "c_player_root";
        private string _playerNodeName = "c_player";
        private string _spawnNodeName = "c_player_spawn";
        private bool _canReuseExisting = true; // 기존 플레이어 재사용 여부

        private PlayerController _player;
        private Transform _playerRoot;
        private Transform _spawnPoint;

        protected override bool OnCreateMono()
        {
            // 1) 플레이어 루트 탐색 (없으면 Room 자기 자신 사용)
            if (UNITY.TryFindGOWithName(out GameObject rootGo, _playerRootName, this.gameObject, false))
                _playerRoot = rootGo.transform;
            else
                _playerRoot = this.transform;

            // 2) 스폰 포인트 탐색 (없으면 루트 기준)
            if (UNITY.TryFindGOWithName(out GameObject spGo, _spawnNodeName, _playerRoot.gameObject, false))
                _spawnPoint = spGo.transform;
            else
                _spawnPoint = _playerRoot;

            // 3) 기존 플레이어 재사용
            if (_canReuseExisting &&
                UNITY.TryFindGOWithName(out GameObject existed, _playerNodeName, _playerRoot.gameObject, false))
            {
                if (!UNITY.TryGetComp(out _player, existed))
                {
                    LOG.Error($"PlayerSpawner: '{_playerNodeName}' exists but PlayerController missing");
                    return false;
                }
                return true;
            }

            // 4) 프리팹에서 생성
            if (_playerPrefab == null)
            {
                LOG.Error("PlayerSpawner: _playerPrefab is null");
                return false;
            }

            GameObject inst = GameObject.Instantiate(
                _playerPrefab,
                _spawnPoint.position,
                _spawnPoint.rotation,
                _playerRoot
            );
            inst.name = _playerNodeName;

            if (!UNITY.TryGetComp(out _player, inst))
            {
                LOG.Error("PlayerSpawner: PlayerController not found on instantiated prefab");
                GameObject.Destroy(inst);
                return false;
            }

            _player.SetIsCreateInRuntime(true);   // 런타임 생성 표식
            return true;
        }

        protected override void OnShowMono()
        {
            if (_player == null) return;
            _player.Show();
        }

        protected override void OnHideMono()
        {
            if (_player == null) return;
            _player.Hide();
        }

        protected override void OnDestroyMono()
        {
            if (_player != null && _player.IsCreateInRuntime)
                UNITY.DestroyMono(ref _player);

            _playerRoot = null;
            _spawnPoint = null;
        }

        protected override bool IsAutoShow() { return true; }
    }
}
