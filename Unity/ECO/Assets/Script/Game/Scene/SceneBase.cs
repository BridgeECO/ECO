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
        private void OnDestroy()
        {
            OnDestroyScene();
        }

        private void Awake()
        {
            //Main이 없으면  Main 오브젝트 생성

            //하위에서 실패할경우
            if (!OnAwakeScene())
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
            OnFixeUpdateScene();
        }

        protected abstract void OnDestroyScene();
        protected abstract bool OnAwakeScene();
        protected abstract void OnUpdateScene();
        protected abstract void OnFixeUpdateScene();
    }

}