using UnityEngine;

namespace ECO
{
    public class MapSimulatorScene : SceneBase
    {
        private MapSimulatorSceneUIController _uiCtrl = new MapSimulatorSceneUIController();
        private CameraController _camCtrl = new CameraController();
        private MapController _mapCtrl = new MapController();
        private IPlayerController _playerCtr = new MapSimulatorPlayerController();

        protected override bool OnAwakeScene(Canvas canvas, App app)
        {
            if (!_uiCtrl.Create(canvas))
                return false;
            if (!_camCtrl.Create(this.gameObject, app.CfgSys))
                return false;
            if (!_mapCtrl.Create(this.gameObject))
                return false;
            if (!_playerCtr.Create(this.gameObject))
                return false;

            _mapCtrl.ShowMap();
            _mapCtrl.ShowAllRegion();

            _camCtrl.SetFollowTarget(_playerCtr.Player.TF);

            _playerCtr.ShowPlayer();

            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.RightArrow, null, () => _playerCtr.Move(Vector2.right)));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.LeftArrow, null, () => _playerCtr.Move(Vector2.left)));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.UpArrow, null, () => _playerCtr.Move(Vector2.up)));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.DownArrow, null, () => _playerCtr.Move(Vector2.down)));
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