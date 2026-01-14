using System;
using UnityEngine;

namespace ECO
{
    [Serializable]
    public class GameConfig : IConfig
    {
        [SerializeField] public float ResonanceRadius = 500f;

        public void Copy(IConfig newCfg)
        {
            var newGameCfg = newCfg as GameConfig;

            this.ResonanceRadius = newGameCfg.ResonanceRadius;
        }
    }
}
