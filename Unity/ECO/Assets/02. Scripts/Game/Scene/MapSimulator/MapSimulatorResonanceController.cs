using UnityEngine;

namespace ECO
{
    public class MapSimulatorResonanceController : IResonanceController, IDestroyable
    {
        private GameConfig _gameCfg = null;
        private IResonanceObjManager _objMgr = new MapSimulatorResonanceObjManager();
        private Ticker _ticker = null;
        private MapSimulatorResonanceValue _resonanceValue = null;

        public bool Create(GameObject sceneRootGO, App app)
        {
            if (!_objMgr.Create(sceneRootGO))
                return false;

            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.Mouse0, OnClickMouse));
            _gameCfg = app.CfgSys.GetConfig<GameConfig>();
            _ticker = app.TickerMgr.GetTicker();
            _ticker.SetOnTickAct(OnTick);

            return true;
        }

        public void Destroy()
        {

        }

        private void OnClickMouse()
        {
            if (_resonanceValue != null)
                return;

            var radius = _gameCfg.ResonanceRadius;
            var centerPos = CalcaResonanceMousePos();
            var objInCircleList = _objMgr.FindObjListInCircle(centerPos, radius);

            if (objInCircleList.Count <= 0)
                return;

            objInCircleList.ForEach(x => x.ActivateResonance());

            _resonanceValue = new MapSimulatorResonanceValue(centerPos, radius, objInCircleList);
            _resonanceValue.SetRadius(0f);
            _resonanceValue.SetIsInc(true);
        }

        private void OnTick()
        {
            if (_resonanceValue == null)
                return;

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

        private Vector3 CalcaResonanceMousePos()
        {
            var mousePos = Input.mousePosition;
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
            return worldMousePos;
        }
    }
}