namespace ECO
{
    public class DropDownBtnInfo
    {
        public DropDownBtnInfo(int idx, string txt, bool isDefault)
        {
            this.Idx = idx;
            this.Txt = txt;
            this.IsDefault = isDefault;
        }

        public int Idx { get; private set; } = CONST.INVALID_IDX;
        public string Txt { get; private set; } = "";
        public bool IsDefault { get; private set; } = false;
    }
}