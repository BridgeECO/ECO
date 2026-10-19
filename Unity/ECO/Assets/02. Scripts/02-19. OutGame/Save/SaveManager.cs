using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// 파일 입출력과 현재 세이브 데이터 보관만 담당하는 순수 C# 클래스.
// Unity 생명주기와 씬 오브젝트에 의존하지 않으므로 MonoBehaviour가 아닌 POCO 싱글턴으로 관리한다.
public class SaveManager : POCOSingleton<SaveManager>
{
    private const string SAVE_FILE_NAME_FORMAT = "saveData_{0}.json";

    public int CurrentSlotIndex { get; set; }
    public SaveData CurrentSaveData { get; private set; }

    public SaveManager()
    {
        CurrentSaveData = new SaveData();
    }

    public void ResetCurrentSaveData()
    {
        CurrentSaveData = null;
    }

    // 이어하기를 확정한 시점에만 지정한다. Load가 이 값을 바꾸면 슬롯 목록을 새로 그리기만 해도
    // 이어하기 대상이 마지막으로 읽은 슬롯으로 바뀌어, 새 게임이 다른 슬롯의 위치에서 시작된다.
    public void SetCurrentSaveData(SaveData saveData)
    {
        CurrentSaveData = saveData;
    }

    // 도메인 리로드 비활성화 환경에서 플레이 세션 간 상태 잔류 방지
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitStaticState()
    {
        ResetInstance();
    }

    /// <summary>
    /// 세이브 포인트 좌표를 현재 슬롯에 기록한다. 실제로 파일을 쓴 경우에만 true를 반환한다.
    /// 씬/지역 컨텍스트는 호출자(RespawnManager)가 수집해 전달한다.
    /// (SaveManager가 Region/SceneTransitionManager를 직접 조회하지 않기 위함)
    /// </summary>
    public bool Save(ESceneNames sceneName, ERegions region, Vector3 savePointPosition)
    {
        if (CurrentSaveData is null)
        {
            CurrentSaveData = new SaveData();
        }

        CurrentSaveData.SceneName = sceneName;
        CurrentSaveData.Region = region;
        CurrentSaveData.SavePointPosition = savePointPosition;
        Save(CurrentSlotIndex, CurrentSaveData.ToDTO());
        return true;
    }

    public void Save(int slotIndex, SaveDataDTO dto)
    {
        if (dto is null)
        {
            return;
        }

        string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
        string filePath = GetSaveFilePath(slotIndex);

        string directoryPath = Path.GetDirectoryName(filePath);
        if (Directory.Exists(directoryPath) == false)
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(filePath, json);
    }

    public void DeleteSave(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public SaveData Load(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);

        if (File.Exists(filePath) == false)
        {
            return null;
        }
        string json = File.ReadAllText(filePath);
        SaveDataDTO dto = JsonConvert.DeserializeObject<SaveDataDTO>(json);

        if (dto is null)
        {
            return null;
        }
        return new SaveData(dto.SceneName, dto.Region, dto.SavePointPosition);
    }

    private string GetSaveFilePath(int slotIndex)
    {
        string fileName = string.Format(SAVE_FILE_NAME_FORMAT, slotIndex);
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "21. SaveFiles", fileName);
#else
        return Path.Combine(Application.persistentDataPath, fileName);
#endif
    }
}
