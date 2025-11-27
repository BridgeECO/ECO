using UnityEngine;

namespace ECO
{
    public class TestTutorialScene : SceneBase
    {
        private CameraController _camCtrl = new CameraController();
        private PlayerController _playerCtrl = new PlayerController();
        private MapController _mapCtrl = new MapController();

        protected override bool OnAwakeScene(Canvas canvas, App app)
        {
            if (!_camCtrl.Create(this.gameObject, app.CfgSys))
                return false;

            if (!_mapCtrl.Create(this.gameObject))
                return false;

            if (!_playerCtrl.Create(this.gameObject))
                return false;

            _mapCtrl.ShowMap();
            _mapCtrl.ShowAllRegion();

            _playerCtrl.ShowPlayer();
            _camCtrl.SetFollowTarget(_playerCtrl.Player.TF);

            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.Space, null, () => _playerCtrl.Jump()));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.W, null, () => _playerCtrl.Jump()));
            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.UpArrow, null, () => _playerCtrl.Jump()));

            return true;
        }

        protected override void OnUpdateScene()
        {
            _playerCtrl.Update();
        }

        protected override void OnFixedUpdateScene()
        {
        }

        protected override void OnDestroyScene()
        {
            _camCtrl.Destroy();
            _mapCtrl.Destroy();
        }
    }
}
