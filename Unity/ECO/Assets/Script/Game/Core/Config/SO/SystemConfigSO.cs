using System;
using UnityEngine;

namespace ECO
{
    [CreateAssetMenu(fileName = "SystemConfigSO", menuName = "Scriptable Objects/SystemConfigSO")]
    public class SystemConfigSO : ConfigSOBase<SystemConfig>
    {
        [field: SerializeField]
        public int PixelPerUnit { get; private set; }


        private SystemConfig _cfg = new SystemConfig();

        public override SystemConfig BuildCfg()
        {
            _cfg.Build(this);
            return _cfg;
        }

        public override Type GetCfgType()
        {
            return typeof(SystemConfig);
        }
    }
}