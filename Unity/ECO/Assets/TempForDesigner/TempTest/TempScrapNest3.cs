using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace ECO
{
    public class TempScrapNest3 : MonoBase
    {
        private Transform _tempDeathObject = null;
        private Vector3 _deathObjectOriginalPos;

        private TempResetRoom _tempResetRoom;

        public bool isEnd;
        public bool isStart;
        public float moveSpeed;

        protected override bool OnCreateMono()
        {
            GameObject tempDeath;
            UNITY.TryFindGOWithName(out tempDeath, "TempDeathObject", rootGO: gameObject);
            _tempDeathObject = tempDeath.transform;
            _deathObjectOriginalPos = _tempDeathObject.position;

            _tempResetRoom = GetComponent<TempResetRoom>();

            isEnd = false;
            isStart = false;

            moveSpeed = 3.5f;
            
            StartCoroutine("MovingDeathObject");

            return true;
        }

        public IEnumerator MovingDeathObject()
        {
            while(isStart == false)
            {
                //Debug.Log("코루틴 멈춰있음");
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("추격자 추격 시작");

            while(isEnd == false)
            {
                yield return null;
                _tempDeathObject.Translate(0, moveSpeed * Time.deltaTime, 0);
            }

            //isEnd가 True로 변하면 실행되는 코드
            isStart = false;
            _tempDeathObject.position = _deathObjectOriginalPos;
            _tempResetRoom.ResetRoom();
            OnEnable();
        }

        protected override void OnDestroyMono()
        {
            
        }

        protected override bool IsAutoShow()
        {
            return true;
        }

        void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine("MovingDeathObject");
            isEnd = false;
        }
    }
}
