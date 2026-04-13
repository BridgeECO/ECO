using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SaveManager : MonoBehaviourSingleton<SaveManager>
{
    public int CurrentSlotIndex { get; set; }
    public SaveData CurrentSaveData { get; private set; }

    private const string SaveFileNameFormat = "saveData_{0}.json";

    protected override void Awake()
    {
        base.Awake();
        CurrentSaveData = new SaveData();
    }

    public void Save(Room targetRoom)
    {
        Region region = Region.Instance;
        if (CurrentSaveData == null || region == null)
        {
            return;
        }
        CurrentSaveData.Region = region.RegionType;
        CurrentSaveData.RoomIndex = region.GetRoomIndex(targetRoom);
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
        SaveData saveData = new SaveData(dto.Region, dto.RoomIndex);
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
