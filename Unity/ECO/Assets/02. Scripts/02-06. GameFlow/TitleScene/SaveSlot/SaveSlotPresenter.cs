using Cysharp.Threading.Tasks;

/// <summary>
/// 저장 슬롯 하나의 유즈케이스(이어하기/새 게임/삭제) 오케스트레이션.
/// 뷰(UI_SaveSlotItem)가 세이브·라이프·씬 전환·팝업·사운드 매니저를
/// 직접 호출하지 않도록 분리한 프레젠터. 뷰는 표시와 입력만 담당한다.
/// </summary>
public class SaveSlotPresenter
{
    private readonly int _slotIndex;
    private readonly ESceneNames _newGameScene;

    public SaveSlotPresenter(int slotIndex, ESceneNames newGameScene)
    {
        _slotIndex = slotIndex;
        _newGameScene = newGameScene;
    }

    public SaveData LoadSlotData()
    {
        return SaveManager.Instance.Load(_slotIndex);
    }

    public void DeleteSlot()
    {
        SaveManager.Instance.DeleteSave(_slotIndex);
    }

    /// <summary>저장 데이터가 있으면 해당 씬으로 이어하기를 시작한다.</summary>
    public bool TryContinue()
    {
        SaveData saveData = LoadSlotData();
        if (saveData is null)
        {
            return false;
        }

        SaveManager.Instance.CurrentSlotIndex = _slotIndex;
        LifeManager.Instance.SetLifeToMax();
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(saveData.SceneName).Forget();
        return true;
    }

    /// <summary>
    /// 새 게임을 시작한다. 기존 데이터가 있는 슬롯이면 덮어쓰기 확인 팝업을 거치며,
    /// 플레이어가 거절하면 false를 반환하고 아무것도 하지 않는다.
    /// </summary>
    public async UniTask<bool> TryStartNewGameAsync(bool hasSaveData)
    {
        if (hasSaveData)
        {
            bool isConfirmed = await UIManager.Instance.ShowNewGameConfirmAsync();
            if (!isConfirmed)
            {
                return false;
            }
            DeleteSlot();
        }

        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Title_StartGame);
        }
        SaveManager.Instance.CurrentSlotIndex = _slotIndex;
        SaveManager.Instance.ResetCurrentSaveData();
        LifeManager.Instance.SetLifeToMax();
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(_newGameScene).Forget();
        return true;
    }
}
