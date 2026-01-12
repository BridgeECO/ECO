using UnityEngine;

namespace ECO
{
    public class TempEnterArea : MonoBase
    {
        private TempResetRoom _parentRoom = null; 
        
        protected override bool OnCreateMono()
        {
            _parentRoom = transform.parent.parent.GetComponent<TempResetRoom>();
            
            return true;
        }

        protected override void OnDestroyMono()
        {
            
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(!collision.CompareTag("Player"))
                return;

            //다른 모든 TempResetRoom의 isNowRoom을 false로 변경
            foreach (TempResetRoom tempReset in _parentRoom.transform.parent.GetComponentsInChildren<TempResetRoom>())
            {
                tempReset.isNowRoom = false;
            }

            //현재 진입한 TempResetRoom의 isNowRoom을 true로 변경
            _parentRoom.isNowRoom = true;
        }

        protected override bool IsAutoShow()
        {
            return true;
        }
    }
}
