using UnityEngine;

namespace ECO
{
    public class TempStartRacingLine : MonoBase
    {
        private TempScrapNest3 tempScrapNest3 = null;
        
        protected override bool OnCreateMono()
        {
            tempScrapNest3 = transform.parent.parent.GetComponent<TempScrapNest3>();
            
            return true;
        }

        protected override void OnDestroyMono()
        {
            
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.tag == "Player")
            {
                tempScrapNest3.isStart = true;
            }
        }

        protected override bool IsAutoShow()
        {
            return true;
        }
    }
}
