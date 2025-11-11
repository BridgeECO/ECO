using UnityEngine;

namespace ECO
{
    public class MapSimulatorScene : SceneBase
    {
        private MapSimulatorSceneUIController _uiCtrl = new MapSimulatorSceneUIController();

        protected override bool OnAwakeScene(Canvas canvas, App app)
        {
            if (!_uiCtrl.Create(canvas))
                return false;

            return true;
        }

        protected override void OnDestroyScene()
        {
            _uiCtrl.Destroy();
        }

        protected override void OnFixedUpdateScene()
        {

        }

        protected override void OnUpdateScene()
        {

        }
    }
}