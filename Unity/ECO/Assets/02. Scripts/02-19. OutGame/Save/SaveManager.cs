using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// 파일 입출력과 현재 세이브 데이터 보관만 담당하는 순수 C# 클래스.
// Unity 생명주기와 씬 오브젝트에 의존하지 않으므로 MonoBehaviour가 아닌 POCO 싱글턴으로 관리한다.
public class SaveManager : POCOSingleton<SaveManager>
{
    public int CurrentSlotIndex { get; set; }
    public SaveData CurrentSaveData { get; private set; }

    private const string SaveFileNameFormat = "saveData_{0}.json";

    public SaveManager()
    {
        CurrentSaveData = new SaveData();
    }

    public void ResetCurrentSaveData()
    {
        CurrentSaveData = null;
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
        if (dto == null)
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

        if (dto == null)
        {
            return null;
        }
        SaveData saveData = new SaveData(dto.SceneName, dto.Region, dto.SavePointPosition);
        CurrentSaveData = saveData;
        return saveData;
    }

    private string GetSaveFilePath(int slotIndex)
    {
        string fileName = string.Format(SaveFileNameFormat, slotIndex);
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "21. SaveFiles", fileName);
#else
        return Path.Combine(Application.persistentDataPath, fileName);
#endif
    }
}
