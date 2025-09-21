using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ECO
{
    public static class UNITY
    {
        public static bool IsNullGameObj(GameObject go)
        {
            return Object.ReferenceEquals((object)go, null);
        }

        public static bool IsNullMonoBase(MonoBase go)
        {
            return Object.ReferenceEquals(go, null);
        }

        public static bool IsNullComp(Component comp)
        {
            return Object.ReferenceEquals(comp, null);
        }


        public static void DestroyMonoList<T>(ref List<T> monoList) where T : MonoBase
        {
            for (int i = 0; i < monoList.Count; i++)
            {
                T mono = monoList[i];
                DestroyMono(ref mono);
            }

            monoList.Clear();
        }

        public static void DestroyMonoQueue<T>(ref Queue<T> monoQueue) where T : MonoBase
        {
            while (monoQueue.Count > 0)
            {
                T mono = monoQueue.Dequeue();
                DestroyMono(ref mono);
            }

            monoQueue.Clear();
        }

        public static void DestroyMono<T>(ref T mono) where T : MonoBase
        {
            if (mono == null)
                return;

            mono.Destroy();

            if (!mono.IsCreateInRuntime)
            {
                mono = null;
                return;
            }

            GameObject.DestroyImmediate(mono.gameObject);

            if (Application.isPlaying)
                GameObject.Destroy(mono);
            else
                GameObject.DestroyImmediate(mono);
        }


        public static bool TryGetComp<T>(out T comp, GameObject go, bool isShowErr = true)
        {
            comp = default(T);

            if (IsNullGameObj(go))
            {
                if (isShowErr)
                    LOG.E($"Invalid GameObject");

                return false;
            }

            if (!go.TryGetComponent<T>(out comp))
            {
                if (isShowErr)
                    LOG.E($"Not Found Component. GameObject({go.name}), Comp({typeof(T)})");

                return false;
            }

            return true;
        }

        public static bool TryFindCompWithName<T>(out T comp, string name, GameObject rootGO = null, bool isShowErr = true) where T : Component
        {
            comp = null;

            //게임오브젝트 먼저 찾기
            if (!TryFindGOWithName(out GameObject go, name, rootGO, isShowErr))
                return false;

            //게임오브젝트에서 Component 찾기
            if (!TryGetComp(out comp, go, isShowErr))
                return false;

            if (comp is MonoBase monoBaseComp)
                return monoBaseComp.Create();

            return true;
        }

        public static bool TryFindGOWithName(out GameObject go, string name, GameObject rootGO = null, bool isShowErr = true)
        {
            if (rootGO == null)
                go = GameObject.Find(name);
            else
                go = FindGOWithName(rootGO, name);

            if (!IsNullGameObj(go))
                return true;

            Scene curScene = SceneManager.GetActiveScene();
            go = FindGOInSceneWithName(curScene, name);

            if (!IsNullGameObj(go))
                return true;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                go = FindGOInSceneWithName(SceneManager.GetSceneAt(i), name);

                if (!IsNullGameObj(go))
                    return true;
            }

            if (isShowErr)
                LOG.E($"Not Found GameObject. GameObject({name})");

            return false;
        }
        private static GameObject FindGOInSceneWithName(Scene scene, string path)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.name == path)
                    return go;

                Transform tf = go.transform.Find(path);

                if (tf == null)
                    continue;

                return tf.gameObject;
            }

            return null;
        }

        private static GameObject FindGOWithName(GameObject rootGO, string name)
        {
            for (int i = 0; i < rootGO.transform.childCount; i++)
            {
                Transform child = rootGO.transform.GetChild(i);
                if (child.gameObject.name == name)
                    return child.gameObject;

                GameObject childOfchild = FindGOWithName(child.gameObject, name);

                if (childOfchild != null)
                    return childOfchild;
            }

            return null;
        }
    }
}