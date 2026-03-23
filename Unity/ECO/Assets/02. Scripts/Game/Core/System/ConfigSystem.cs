using System;
using System.Collections.Generic;

namespace ECO
{
    public class ConfigSystem : IDestroyable
    {
        private Dictionary<Type, IConfig> _configDict = new Dictionary<Type, IConfig>();

        public void Destroy()
        {
            _configDict.Clear();
        }

        public bool Create()
        {
            return LoadAllCfgSOAndBuild();
        }

        public CFG GetConfig<CFG>() where CFG : class, IConfig, new()
        {
            if (_configDict.TryGetValue(typeof(CFG), out IConfig cfg))
                return cfg as CFG;

            cfg = new CFG();
            _configDict.Add(typeof(CFG), cfg);
            return cfg as CFG;
        }

        private bool LoadAllCfgSOAndBuild()
        {
            if (!LoadCfgSOAndBuild<SystemConfigSO, SystemConfig>("system"))
                return false;
            if (!LoadCfgSOAndBuild<GameConfigSO, GameConfig>("game"))
                return false;

            return true;
        }

        private bool LoadCfgSOAndBuild<CFGSO, CFG>(string cfgName) where CFGSO : ConfigSOBase<CFG> where CFG : class, IConfig, new()
        {
            if (!RES.TryLoadAddressableAsset<CFGSO>(out var cfgSO, PATH.GetCfgSOAddressableKey(cfgName)))
                return false;

            var cfg = cfgSO.BuildCfg();
            _configDict.Add(cfg.GetType(), cfgSO.BuildCfg());
            cfgSO.OnBuildCfg = OnBuildCfg<CFG>;
            return true;
        }

        private void OnBuildCfg<CFG>(CFG newCfg) where CFG : class, IConfig, new()
        {
            IConfig oldCFG = GetConfig<CFG>();
            oldCFG.Copy(newCfg);
        }
    }
}