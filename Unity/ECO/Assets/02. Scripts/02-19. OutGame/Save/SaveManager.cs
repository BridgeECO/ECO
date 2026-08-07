using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviourSingleton<SaveManager>
{
    public int CurrentSlotIndex { get; set; }
    public SaveData CurrentSaveData { get; private set; }

    private const string SaveFileNameFormat = "saveData_{0}.json";

    public void ResetCurrentSaveData()
    {
        CurrentSaveData = null;
    }

    protected override void Awake()
    {
        base.Awake();
        CurrentSaveData = new SaveData();
    }

    public void Save(Vector3 savePointPosition)
    {
        Region region = Region.Instance;
        if (region == null)
        {
            return;
        }

        if (CurrentSaveData is null)
        {
            CurrentSaveData = new SaveData();
        }

        string activeSceneName = SceneTransitionManager.Instance != null ? SceneTransitionManager.Instance.CurrentLoadedRegionScene : null;
        if (string.IsNullOrEmpty(activeSceneName) || !System.Enum.TryParse(activeSceneName, out ESceneNames sceneName))
        {
            // TitleScene으로 폴백하면 이어하기가 타이틀로 진입하는 슬롯이 만들어지므로 저장 자체를 중단한다.
            Debug.LogError($"현재 씬 이름('{activeSceneName}')을 ESceneNames로 변환할 수 없어 저장을 건너뜁니다.");
            return;
        }

        CurrentSaveData.SceneName = sceneName;
        CurrentSaveData.Region = region.RegionType;
        CurrentSaveData.SavePointPosition = savePointPosition;
        Save(CurrentSlotIndex, CurrentSaveData.ToDTO());
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
