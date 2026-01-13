using UnityEngine;

namespace ECO
{
    public class TempDeathObject : MonoBase, IDestroyable
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

        protected override bool IsAutoShow()
        {
            return true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.tag == "Player")
            {
                tempScrapNest3.isEnd = true;
            }
        }
    }
}
