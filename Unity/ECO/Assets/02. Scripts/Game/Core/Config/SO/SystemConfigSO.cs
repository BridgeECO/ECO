using System;
using UnityEngine;

namespace ECO
{
    [CreateAssetMenu(fileName = "SystemConfigSO", menuName = "Scriptable Objects/SystemConfigSO")]
    public class SystemConfigSO : ConfigSOBase<SystemConfig>
    {
        [field: SerializeField]
        public SystemConfig _cfg { get; private set; } = new SystemConfig();

        public override SystemConfig BuildCfg()
        {
            return _cfg;
        }

        public override Type GetCfgType()
        {
            return typeof(SystemConfig);
        }
    }
}