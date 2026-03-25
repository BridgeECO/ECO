using System.Collections.Generic;
using UnityEngine;

namespace ECO
{
    public class MapSimulatorResonanceObjManager : IResonanceObjManager
    {
        private List<ResonanceObject> _resonanceObjList = new List<ResonanceObject>();

        public bool Create(GameObject sceneRootGO)
        {
            var objArr = Object.FindObjectsByType(typeof(ResonanceObject), FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in objArr)
            {
                if (obj is ResonanceObject resonanceObj)
                    _resonanceObjList.Add(resonanceObj);
            }

            return true;
        }

        public List<ResonanceObject> FindObjListInCircle(Vector2 centerPos, float radius)
        {
            float radiusSqr = radius * radius;
            List<ResonanceObject> result = new List<ResonanceObject>();

            foreach (var obj in _resonanceObjList)
            {
                Bounds bounds = obj.GetColBound();

                float closestX = Mathf.Clamp(centerPos.x, bounds.min.x, bounds.max.x);
                float closestY = Mathf.Clamp(centerPos.y, bounds.min.y, bounds.max.y);

                Vector2 closestPoint = new Vector2(closestX, closestY);

                if ((closestPoint - centerPos).sqrMagnitude <= radiusSqr)
                {
                    result.Add(obj);
                }
            }

            return result;
        }
    }
}