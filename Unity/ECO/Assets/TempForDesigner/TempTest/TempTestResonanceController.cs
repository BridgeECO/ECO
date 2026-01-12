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

        private List<ResonanceObject> _objInCircleList = null;

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
            _objInCircleList = _objMgr.FindObjListInCircle(playerTransform.position, _resonanceValue.CurRadius);
            _objInCircleList.ForEach(x => x.ActivateResonance());
            Debug.Log(_objInCircleList.Count.ToString());

            if (_resonanceValue.IsInc)
            {
                _resonanceValue.IncRadius(0.1f);

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