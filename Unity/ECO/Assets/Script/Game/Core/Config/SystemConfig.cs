using System;
using UnityEngine;

namespace ECO
{
    [Serializable]
    public class SystemConfig : IConfig
    {
        [field: SerializeField]
        public int DefaultResolutionX { get; private set; }
        [field: SerializeField]
        public int DefaultResolutionY { get; private set; }
        [field: SerializeField]
        public int PixelPerUnit { get; private set; }

        public void Copy(IConfig newCfg)
        {
            var newSysCfg = newCfg as SystemConfig;

            this.PixelPerUnit = newSysCfg.PixelPerUnit;
        }
    }
}