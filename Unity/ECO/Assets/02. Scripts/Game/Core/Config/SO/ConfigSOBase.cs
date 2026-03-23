using System;
using UnityEngine;
using UnityEngine.Events;


namespace ECO
{
    public abstract class ConfigSOBase<IConfig> : ScriptableObject
    {
        protected void Refresh()
        {
            var cfg = BuildCfg();
            OnBuildCfg?.Invoke(cfg);
        }

        public abstract IConfig BuildCfg();
        public abstract Type GetCfgType();

        public UnityAction<IConfig> OnBuildCfg;
    }
}