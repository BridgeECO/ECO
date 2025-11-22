using Unity.Cinemachine;
using UnityEngine;

namespace ECO
{
    public class CameraController : IDestroyable
    {
        private CinemachineCamera _cinemachineCam = null;

        public bool Create(GameObject sceneRootGO)
        {
            if (!UNITY.TryFindGOWithName(out GameObject gameCamGO, "c_cam", sceneRootGO))
                return false;

            if (!UNITY.TryFindCompWithName(out _cinemachineCam, "c_cinemachine_cam", gameCamGO))
                return false;

            return true;
        }

        public void Destroy()
        {
            _cinemachineCam = null;
        }
    }
}