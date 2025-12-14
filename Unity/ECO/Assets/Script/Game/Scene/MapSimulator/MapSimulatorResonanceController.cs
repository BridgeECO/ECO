using UnityEngine;

namespace ECO
{
    public class MapSimulatorResonanceController : IResonanceController, IDestroyable
    {
        private GameConfig _gameCfg = null;
        private IResonanceObjManager _objMgr = new MapSimulatorResonanceObjManager();

        public bool Create(GameObject sceneRootGO, App app)
        {
            if (!_objMgr.Create(sceneRootGO))
                return false;

            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.Mouse0, OnClickMouse));
            _gameCfg = app.CfgSys.GetConfig<GameConfig>();

            return true;
        }

        public void Destroy()
        {

        }

        private void OnClickMouse()
        {
            var radius = _gameCfg.ResonanceRadius;
            var centerPos = CalcaResonanceMousePos();
            var objInCircleList = _objMgr.FindObjListInCircle(centerPos, radius);

            foreach (var obj in objInCircleList)
            {
                obj.SetCircleParams(centerPos, radius);
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