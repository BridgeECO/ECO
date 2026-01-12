using System.Collections.Generic;
using UnityEngine;

namespace ECODebug
{
    public static class DebugSystem
    {
        private static GameObject _rootGO = null;

        private static List<DebugObjectBase> _freeObjList = new List<DebugObjectBase>();
        private static List<DebugObjectBase> _allocObjList = new List<DebugObjectBase>();

        public static int GenerateSN(GameObject go)
        {
            return go != null ? go.GetInstanceID() : -1;
        }

        public static void Destroy()
        {
            _freeObjList.ForEach(x => x.Destroy());
            _allocObjList.ForEach(x => x.Destroy());

            _freeObjList.Clear();
            _allocObjList.Clear();
        }

        public static bool Create()
        {
            _rootGO = new GameObject("ECODebugRoot");
            Object.DontDestroyOnLoad(_rootGO);
            return true;
        }

        public static void DrawBoxCol2D(Collider2D col2D, int sn)
        {
            var box = AllocDebugObj<DebugBoxObject>(sn);
            box.Set(col2D);
        }

        private static T AllocDebugObj<T>(int sn) where T : DebugObjectBase, new()
        {
            for (int i = 0; i < _allocObjList.Count; i++)
            {
                if (_allocObjList[i] is T obj && obj.IsSNEqual(sn))
                {
                    return obj;
                }
            }

            for (int i = 0; i < _freeObjList.Count; i++)
            {
                if (_freeObjList[i] is T obj && obj.IsSNEqual(sn))
                {
                    _freeObjList.RemoveAt(i);
                    _allocObjList.Add(obj);
                    obj.Show();
                    return obj;
                }
            }

            var newObj = new T();
            newObj.Create(_rootGO, sn);
            newObj.Show();

            _allocObjList.Add(newObj);
            return newObj;
        }

        private static void FreeDebugObj(DebugObjectBase obj)
        {
            obj.Hide();
            _allocObjList.Remove(obj);
            _freeObjList.Add(obj);
        }
    }
}