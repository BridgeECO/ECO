using System.Collections.Generic;
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
            _resonanceValue = null;
        }

        public void OnPlayerAirJumped(Transform nowPlayer)
        {
            if (_resonanceValue != null)
                _resonanceValue = null;

            playerTransform = nowPlayer;

            var radius = _gameCfg.ResonanceRadius;
            var centerPos = nowPlayer.position;
            _objInCircleList = _objMgr.FindObjListInCircle(centerPos, radius);

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

            _resonanceValue.CenterPos = playerTransform.position;

            //시각 효과용 공명 플랫폼 리스트 갱신
            _objInCircleList = _objMgr.FindObjListInCircle(playerTransform.position, _resonanceValue.CurRadius);
            
            //음악 연주용 공명 플랫폼 리스트 갱신 및 연주
            _objInSoundList = _objMgr.FindObjListInCircle(playerTransform.position, _resonanceValue.CurRadius/3f);
            _objInSoundList.ForEach(x => x.ActivateResonance());
            Debug.Log(_objInSoundList.Count.ToString());

            if (_resonanceValue.IsInc)
            {
                _resonanceValue.IncRadius(10f);

                if (_resonanceValue.CurRadius >= _resonanceValue.MaxRadius)
                    _resonanceValue.SetIsInc(false);
            }
            else
            {
                _resonanceValue.DecRadius(0.1f);

                if (_resonanceValue.CurRadius <= 0f)
                    _resonanceValue = null;
            }
        }
    }
}