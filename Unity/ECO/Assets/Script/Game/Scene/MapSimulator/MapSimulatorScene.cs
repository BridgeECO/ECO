using UnityEngine;

namespace ECO
{
    public class MapSimulatorScene : SceneBase
    {
        private MapSimulatorSceneUIController _uiCtrl = new MapSimulatorSceneUIController();
        private CameraController _camCtrl = new CameraController();
        private MapController _mapCtrl = new MapController();

        protected override bool OnAwakeScene(Canvas canvas, App app)
        {
            if (!_uiCtrl.Create(canvas))
                return false;
            if (!_camCtrl.Create(this.gameObject))
                return false;
            if (!_mapCtrl.Create(this.gameObject))
                return false;

            _mapCtrl.ShowMap();
            _mapCtrl.ShowAllRegion();
            return true;
        }

        protected override void OnDestroyScene()
        {
            _uiCtrl.Destroy();
            _camCtrl.Destroy();
            _mapCtrl.Destroy();
        }

        protected override void OnFixedUpdateScene()
        {

        }

        protected override void OnUpdateScene()
        {

        }
    }
}