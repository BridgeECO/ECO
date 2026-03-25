using System.Collections.Generic;
using UnityEngine;

namespace ECO
{
    public interface IResonanceObjManager
    {
        public bool Create(GameObject sceneRootGO);
        public List<ResonanceObject> FindObjListInCircle(Vector2 centerPos, float radius);
    }
}