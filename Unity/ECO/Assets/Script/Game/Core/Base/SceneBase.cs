/*
 * 각 씬 (타이틀/로비/게임) 을 담당
 * + 테스트 씬 (Simulator) 이걸 쓸 수 있게 해야 함 (디버그용)
 * 
 */



using UnityEngine;

namespace ECO
{
    public abstract class SceneBase : MonoBehaviour
    {
        private App _app = null;
        private Canvas _canvas = null;

        private void OnDestroy()
        {
            OnDestroyScene();

            _app = null;
        }

        private void Awake()
        {
            _app = AppBootStrapper.Create();

            if (!UNITY.TryFindCompWithName(out _canvas, "c_canvas", this.gameObject))
                return;

            //하위에서 실패할경우
            if (!OnAwakeScene(_canvas, _app))
            {
                this.gameObject.SetActive(false);
                //TODO : 로그 찍기 - 임현준
                return;
            }
        }

        private void Update()
        {
            OnUpdateScene();
        }

        private void FixedUpdate()
        {
            OnFixedUpdateScene();
        }

        protected abstract void OnDestroyScene();
        protected abstract bool OnAwakeScene(Canvas canvas, App app);
        protected abstract void OnUpdateScene();
        protected abstract void OnFixedUpdateScene();
    }

}