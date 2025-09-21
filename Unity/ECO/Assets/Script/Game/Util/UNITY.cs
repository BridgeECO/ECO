using UnityEngine;

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
    }
}