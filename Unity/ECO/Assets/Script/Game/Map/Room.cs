using System.Diagnostics;

namespace ECO
{
    public class Room : MonoBase
    {
        protected override bool OnCreateMono()
        {
            return true;
        }

        protected override void OnDestroyMono()
        {

        }

        protected override bool IsAutoShow()
        {
            return true;
        }
    }
}