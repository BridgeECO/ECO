namespace ECO
{
    public class SystemConfig : IConfig
    {
        public int PixelPerUnit { get; private set; }

        public void Build(SystemConfigSO so)
        {
            this.PixelPerUnit = so.PixelPerUnit;
        }

        public void Copy(IConfig newCfg)
        {
            var newSysCfg = newCfg as SystemConfig;

            this.PixelPerUnit = newSysCfg.PixelPerUnit;
        }
    }
}