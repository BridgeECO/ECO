using UnityEngine;

namespace ECO
{
    public class MapSimulatorResonanceController : IResonanceController, IDestroyable
    {
        private ResonanceObject _resonanceObj = null;

        public bool Create(GameObject sceneRootGO, App app)
        {
            if (!UNITY.TryFindCompWithName(out _resonanceObj, "c_resonance", sceneRootGO))
                return false;

            app.InputSys.RegisterEvt(new InputKeyEvent(KeyCode.Mouse0, OnClickMouse));
            return true;
        }

        public void Destroy()
        {
            UNITY.DestroyMono(ref _resonanceObj);
        }

        private void OnClickMouse()
        {
            _resonanceObj.SetPos(CalcaResonanceMousePos());
            _resonanceObj.Show();
            _resonanceObj.PlayAnim("active");
        }

        private Vector3 CalcaResonanceMousePos()
        {
            var mousePos = Input.mousePosition;
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
            return worldMousePos;
        }
    }
}