using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ECO
{
    [RequireComponent(typeof(Image))]
    public class ButtonModule : MonoBase, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        //버튼 활성 상태
        private enum EActiveState
        {
            ACTIVE,
            INACTIVE,
        }

        private enum EPressState
        {
            NORMAL,
            PRESS_DOWN,
            PRESS_UP,
            MOUSE_OVER,
            MOUSE_EXIT,
        }

        private UnityEvent _onClickEvt = new UnityEvent();
        private EActiveState _curActiveState = EActiveState.ACTIVE;
        private EPressState _curPressState = EPressState.NORMAL;
        private Image _image = null;

        [SerializeField] private Sprite _activeImg = null;
        [SerializeField] private Sprite _selectImg = null;
        [SerializeField] private Sprite _mouseOverImg = null;
        [SerializeField] private bool _isUseAlphaThresHold = false;

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryGetComp(out _image, this.gameObject))
                return false;

            if (_isUseAlphaThresHold)
            {
                if (!_image.sprite.texture.isReadable)
                {
                    LOG.Error("Set AlphaHitTestMinimum. Failed. Image Should Be Readable");
                    return false;
                }

                _image.alphaHitTestMinimumThreshold = 1.0f;
            }

            return true;
        }

        protected override void OnDestroyMono()
        {
            _onClickEvt.RemoveAllListeners();
        }

        protected override void OnShowMono()
        {
            EnterPressState(EPressState.NORMAL);
        }

        public void AddListener(UnityAction act)
        {
            _onClickEvt.AddListener(act);
        }

        public void RemoveListener(UnityAction act)
        {
            _onClickEvt.RemoveListener(act);
        }

        public void RemoveAllListener()
        {
            _onClickEvt.RemoveAllListeners();
        }

        private void EnterPressState(EPressState state)
        {
            if (_curActiveState != EActiveState.ACTIVE)
                return;

            if (_curPressState == state)
                return;

            _curPressState = state;

            switch (_curPressState)
            {
                case EPressState.NORMAL:
                    _image.sprite = _activeImg;
                    break;
                case EPressState.PRESS_DOWN:
                    _image.sprite = _selectImg;
                    break;
                case EPressState.PRESS_UP:
                    InvokeEvent();
                    EnterPressState(EPressState.NORMAL);
                    break;
                case EPressState.MOUSE_OVER:
                    _image.sprite = _mouseOverImg;
                    break;
                case EPressState.MOUSE_EXIT:
                    EnterPressState(EPressState.NORMAL);
                    break;
                default:
                    LOG.NoHandlingEnum(_curPressState);
                    break;
            }

        }

        private void InvokeEvent()
        {
            _onClickEvt.Invoke();
        }

        private bool IsPointerOverUI(PointerEventData eventData)
        {
            List<RaycastResult> resultList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, resultList);

            return false;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (IsPointerOverUI(eventData))
                return;

            EnterPressState(EPressState.PRESS_DOWN);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (IsPointerOverUI(eventData))
                return;

            EnterPressState(EPressState.PRESS_DOWN);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (IsPointerOverUI(eventData))
                return;

            EnterPressState(EPressState.MOUSE_OVER);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (IsPointerOverUI(eventData))
                return;

            EnterPressState(EPressState.MOUSE_EXIT);
        }
    }
}