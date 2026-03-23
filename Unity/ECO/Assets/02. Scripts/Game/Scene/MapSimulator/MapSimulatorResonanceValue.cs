using System.Collections.Generic;
using UnityEngine;

namespace ECO
{
    public class MapSimulatorResonanceValue
    {
        public MapSimulatorResonanceValue(Vector2 centerPos, float maxRadius, List<ResonanceObject> objList)
        {
            this.CenterPos = centerPos;
            this.MaxRadius = maxRadius;
            _objList = objList;
        }

        public float MaxRadius { get; private set; } = 0;
        public float CurRadius { get; private set; } = 0;
        public Vector2 CenterPos { get; private set; } = Vector2.zero;
        public bool IsInc { get; private set; } = false;

        private List<ResonanceObject> _objList = new List<ResonanceObject>();

        public void SetRadius(float radius)
        {
            foreach (var obj in _objList)
            {
                obj.SetCircleParams(this.CenterPos, radius);
            }
        }

        public void IncRadius(float incValue)
        {
            this.CurRadius += incValue;
            if (this.CurRadius >= this.MaxRadius)
                this.CurRadius = this.MaxRadius;

            SetRadius(this.CurRadius);
        }

        public void DecRadius(float decValue)
        {
            this.CurRadius -= decValue;
            SetRadius(this.CurRadius);
        }

        public void SetIsInc(bool isInc)
        {
            this.IsInc = isInc;
        }
    }
}