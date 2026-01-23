using System.Collections.Generic;
using System.Linq;
using Google.Apis.Logging;
using UnityEngine;

namespace ECO
{
    public class TempTestResonanceController : IResonanceController, IDestroyable
    {
        private GameConfig _gameCfg = null;
        private IResonanceObjManager _objMgr = new MapSimulatorResonanceObjManager();
        private Ticker _ticker = null;
        private MapSimulatorResonanceValue _resonanceValue = null;

        //공명 범위에 시각 효과가 발동되는 플랫폼 리스트
        private List<ResonanceObject> _objInCircleList = null;
        //공명 범위에 음향 효과가 발동되는 플랫폼 리스트
        private List<ResonanceObject> _objInSoundList = null;

        private Transform playerTransform;

        public bool Create(GameObject sceneRootGO, App app)
        {
            if (!_objMgr.Create(sceneRootGO))
                return false;

            _gameCfg = app.CfgSys.GetConfig<GameConfig>();
            _ticker = app.TickerMgr.GetTicker();
            _ticker.SetOnTickAct(OnTick);

            return true;
        }

        public void Destroy()
        {
            _ticker.SetActive(false);
            _resonanceValue = null;
        }
        

        public void OnPlayerAirJumped(Transform nowPlayer)
        {
            if (_resonanceValue != null)
                _resonanceValue = null;

            playerTransform = nowPlayer;

            var radius = _gameCfg.ResonanceRadius;
            var centerPos = nowPlayer.position;
            _objInCircleList = _objMgr.FindObjListInCircle(centerPos, 5000f);   //일단 임시로 무지 넓은 지역의 오브젝트 긁어오기
            _objInCircleList.ForEach(x => x.isPlayed = false);

            if (_objInCircleList.Count <= 0)
                return;

            _resonanceValue = new MapSimulatorResonanceValue(centerPos, radius, _objInCircleList);
            _resonanceValue.SetRadius(0f);
            _resonanceValue.SetIsInc(true);
        }

        private void OnTick()
        {
            if (_resonanceValue == null)
                return;
            
            List<ResonanceObject> beforeObjList = _objInCircleList;

            _resonanceValue.CenterPos = playerTransform.position;
            
            // _objInCircleList = _objMgr.FindObjListInCircle(playerTransform.position, _resonanceValue.CurRadius);
            // _resonanceValue._objList = _objInCircleList;
            //_resonanceValue.SetRadius(_resonanceValue.CurRadius);

            _objInCircleList = _objMgr.FindObjListInCircle(playerTransform.position, _resonanceValue.CurRadius);
            
            //beforeObjList에만 있는 플랫폼들은 모두 콜라이더 종료
            foreach (ResonanceObject @object in beforeObjList.Except(_objInCircleList).ToList())
            {
                @object._boxCol.enabled = false;
            }
            //_objInCircleList에 있는 플랫폼들 모두 콜라이더 활성화
            foreach (ResonanceObject @object in _objInCircleList)
            {
                @object._boxCol.enabled = true;
            }

            //음악 연주용 공명 플랫폼 리스트 갱신 및 연주
            _objInSoundList = _objMgr.FindObjListInCircle(playerTransform.position, _resonanceValue.CurRadius/5f);
            _objInSoundList.ForEach(x => x.ActivateResonance());
            
            
            Debug.Log(_objInSoundList.Count.ToString());

            if (_resonanceValue.IsInc)
            {
                _resonanceValue.IncRadius(500f * Time.deltaTime * 40);

                if (_resonanceValue.CurRadius >= _resonanceValue.MaxRadius)
                    _resonanceValue.SetIsInc(false);
            }
            else
            {
                _resonanceValue.DecRadius(0.1f * Time.deltaTime * 200);

                if (_resonanceValue.CurRadius <= 0f)
                    _resonanceValue = null;
            }
        }
    }
}