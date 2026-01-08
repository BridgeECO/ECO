using NUnit.Framework;
using UnityEngine;

namespace ECO
{
    public class TempTestScene : SceneBase
    {
        private CameraController _camCtrl = new CameraController();
        private MapController _mapCtrl = new MapController();
        private PlayerController _playerCtrl = new PlayerController();
        private Player player = null;

        public TempTestResonanceController _resonanceCtrl = new TempTestResonanceController();

        protected override bool OnAwakeScene(Canvas canvas, App app)
        {
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

            _playerCtrl.ShowPlayer();

            return true;
        }
        
        protected override void OnDestroyScene()
        {
            _camCtrl.Destroy();
            _mapCtrl.Destroy();
        }

        protected override void OnFixedUpdateScene()
        {

        }

        protected override void OnUpdateScene()
        {
            _playerCtrl.Update();
        }
    }
}