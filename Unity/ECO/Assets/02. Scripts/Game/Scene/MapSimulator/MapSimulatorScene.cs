using UnityEngine;

namespace ECO
{
    public class MapSimulatorScene : SceneBase
    {
        private MapSimulatorSceneUIController _uiCtrl = new MapSimulatorSceneUIController();
        private CameraController _camCtrl = new CameraController();
        private MapController _mapCtrl = new MapController();

        private IPlayerController _playerCtrl = new MapSimulatorPlayerController();
        private IResonanceController _resonanceCtrl = new MapSimulatorResonanceController();

        protected override bool OnAwakeScene(Canvas canvas, App app)
        {
            if (!_uiCtrl.Create(canvas))
                return false;
            if (!_camCtrl.Create(this.gameObject, app.CfgSys))
                return false;
            if (!_mapCtrl.Create(this.gameObject))
                return false;
            if (!_playerCtrl.Create(this.gameObject))
                return false;
            if (!_resonanceCtrl.Create(this.gameObject, app))
                return false;

            _mapCtrl.ShowMap();
            _mapCtrl.ShowAllRegion();

            _camCtrl.SetFollowTarget(_playerCtrl.Player.TF);

            _playerCtrl.ShowPlayer();

            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.RightArrow, null, () => _playerCtrl.Move(Vector2.right)));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.LeftArrow, null, () => _playerCtrl.Move(Vector2.left)));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.UpArrow, null, () => _playerCtrl.Move(Vector2.up)));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.DownArrow, null, () => _playerCtrl.Move(Vector2.down)));
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