/*
 * 프로그램의 전체 상태 
 * App보단 상위 (게임 보단 상위)
 * 
 * 
 */

using UnityEngine;
using UnityEngine.Events;

namespace ECO
{
    public class AppBootStrapper : MonoBehaviour
    {
        private static App _app = new App();
        private static AppBootStrapper _inst = null;

        private void OnDestroy()
        {
            GameObject.Destroy(_inst);
            _inst = null;
        }

        private void Update()
        {
            _app.Update();
        }

        public static App Create(UnityAction onCreateSuccess = null)
        {
            if (_inst != null)
                return _app;

            var mainRes = Resources.Load<AppBootStrapper>("main");
            _inst = GameObject.Instantiate(mainRes);
            GameObject.DontDestroyOnLoad(_inst);

            if (_app.Create())
                onCreateSuccess?.Invoke();

            return _app;
        }
    }
}