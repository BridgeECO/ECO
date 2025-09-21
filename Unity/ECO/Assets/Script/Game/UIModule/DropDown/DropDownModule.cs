using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ECO
{
    public partial class DropDownModule : MonoBase
    {
        private ButtonModule _defBtn = null;
        private List<ButtonModule> _selectBtnList = new List<ButtonModule>();

        private GameObject _selectGO = null;
        private GameObject _selectBtnRootGO = null;
        private GameObject _selectBtnRes = null;

        private UnityAction<int> _onClickSelectBtn = null;
        private List<DropDownBtnInfo> _btnInfoList = new List<DropDownBtnInfo>();

        private int _curSelectIdx = CONST.INVALID_IDX;
        private bool _isInit = false;

        protected override void OnDestroyMono()
        {
            _defBtn.RemoveListener(EVENT_ClickDefBtn);

            UNITY.DestroyMono(ref _defBtn);
            UNITY.DestroyMonoList(ref _selectBtnList);

            _selectGO = null;
            _selectBtnRootGO = null;
            _isInit = false;
            _onClickSelectBtn = null;
        }

        protected override bool OnCreateMono()
        {
            if (!UNITY.TryFindCompWithName(out _defBtn, "c_def_btn", this.gameObject))
                return false;
            if (!UNITY.TryFindGOWithName(out _selectGO, "c_select", this.gameObject))
                return false;
            if (!UNITY.TryFindGOWithName(out _selectBtnRootGO, "c_select_btn_root", this.gameObject))
                return false;
            if (!UNITY.TryFindGOWithName(out _selectBtnRes, "c_select_btn", this.gameObject))
                return false;

            _selectGO.SetActive(false);
            _defBtn.AddListener(EVENT_ClickDefBtn);
            return true;
        }

        protected override void OnShowMono()
        {
            _defBtn.Show();
            _selectBtnList.ForEach(x => x.Show());
            _selectGO.SetActive(false);

            RefreshDefBtn();
        }

        private void RefreshDefBtn()
        {
            var btnInfo = _btnInfoList[_curSelectIdx];
        }

        protected override void OnHideMono()
        {
            _defBtn.Hide();
            _selectBtnList.ForEach(x => x.Hide());
            _selectGO.SetActive(false);
        }

        private void EVENT_ClickDefBtn()
        {
            _selectGO.SetActive(true);
            _selectBtnList.ForEach(x => x.Show());
        }

        private void EVENT_ClickSelectBtn(int idx)
        {
            _onClickSelectBtn?.Invoke(idx);
            _curSelectIdx = idx;

            _selectGO.SetActive(false);

            RefreshDefBtn();
        }

        private void EVENT_ClickRayCast(List<RaycastResult> resultList)
        {
            if (!_selectGO.activeSelf)
                return;

            foreach (var result in resultList)
            {
                if (result.gameObject == _defBtn.gameObject)
                    return;

                if (result.gameObject == this)
                    return;

                foreach (var btn in _selectBtnList)
                {
                    if (result.gameObject == btn.gameObject)
                        return;
                }
            }

            _selectBtnList.ForEach(x => x.Hide());
            _selectGO.SetActive(false);
        }
    }
}