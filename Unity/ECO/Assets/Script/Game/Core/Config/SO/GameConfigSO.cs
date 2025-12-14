using System;
using UnityEngine;

namespace ECO
{
    [CreateAssetMenu(fileName = "GameConfigSO", menuName = "Scriptable Objects/GameConfigSO")]
    public class GameConfigSO : ConfigSOBase<GameConfig>
    {
        [field: SerializeField]
        public GameConfig _cfg { get; private set; } = new GameConfig();

        public override GameConfig BuildCfg()
        {
            return _cfg;
        }

        public override Type GetCfgType()
        {
            return typeof(GameConfig);
        }
    }
}