using Unity.Cinemachine;
using UnityEngine;

namespace ECO
{
    public class CameraController : IDestroyable
    {
        private CinemachineCamera _cinemachineCam = null;
        private ConfigSystem _cfgSys = new ConfigSystem();

        public bool Create(GameObject sceneRootGO, ConfigSystem cfgSys)
        {
            if (!UNITY.TryFindGOWithName(out GameObject gameCamGO, "c_cam", sceneRootGO))
                return false;
            if (!UNITY.TryFindCompWithName(out _cinemachineCam, "c_cinemachine_cam", gameCamGO))
                return false;

            _cfgSys = cfgSys;

            RefreshCamSize();
            return true;
        }

        public void Destroy()
        {
            _cinemachineCam = null;
        }

        public void SetFollowTarget(Transform tf)
        {
            _cinemachineCam.LookAt = tf;
            _cinemachineCam.Follow = tf;
        }

        private void RefreshCamSize()
        {
            SystemConfig sysCfg = _cfgSys.GetConfig<SystemConfig>();
            int pixelPerUnit = sysCfg.PixelPerUnit;
            int resolutionY = sysCfg.DefaultResolutionY;

            _cinemachineCam.Lens.OrthographicSize = (resolutionY * 0.5f) / pixelPerUnit;
        }
    }
}