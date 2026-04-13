using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SaveManager : POCOSingleton<SaveManager>
{
    public int CurrentSlotIndex { get; set; }
    public SaveData CurrentSaveData { get; private set; }

    private const string SaveFileNameFormat = "saveData_{0}.json";

    private string GetSaveFilePath(int slotIndex)
    {
        string fileName = string.Format(SaveFileNameFormat, slotIndex);
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public void Save(int slotIndex, SaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        SaveDataDTO dto = saveData.ToDTO();

        string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
        string filePath = GetSaveFilePath(slotIndex);

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
}
