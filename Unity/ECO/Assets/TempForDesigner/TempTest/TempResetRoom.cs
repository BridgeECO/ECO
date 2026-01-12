using UnityEngine;
using System.Collections;

namespace ECO
{
    public class TempResetRoom: MonoBase
    {
        private Transform _respawnPoint;
        private Transform _player;

        public bool isNowRoom;

        [SerializeField]
        private GameObject _myselfPrefab;

        protected override bool OnCreateMono()
        {
            GameObject respawnPointObject;
            UNITY.TryFindGOWithName(out respawnPointObject, "TempSpawnPoint", rootGO: gameObject);
            _respawnPoint = respawnPointObject.transform;

            GameObject playerObject;
            UNITY.TryFindGOWithName(out playerObject, "c_player");
            _player = playerObject.transform;
            
            return true;
        }

        protected override void OnDestroyMono()
        {

        }

        private void Update()
        {
            //맵 리셋 테스트용 임시 코드
            if(Input.GetKey(KeyCode.R))
            {
                ResetRoom();
            }
        }

        protected override bool IsAutoShow()
        {
            return true;
        }

        //방 리셋 및 플레이어 리스폰
        //TODO: 미리 저장된 현재 프리펩을 새롭게 소환한 후 현재 프리펩 삭제
        //TODO 안됨. 아무래도 플레이어만 옮긴 다음에, 3번 룸에서 살인하는 특수한 코드를 따로 추가해야 할 듯
        //이러면 진정한 의미의 리셋이 아니지만, 일단 더미코드이므로 어쩔 수 없이 용인.
        public void ResetRoom()
        {
            if(!isNowRoom)
                return;
            
            //var nowPosition = transform.position;
            //var nowRotation = transform.rotation;
            
            //Instantiate(_myselfPrefab, nowPosition, nowRotation, transform.parent);
            _player.position = _respawnPoint.position;

            //Destroy(gameObject);
        }
    }    
}