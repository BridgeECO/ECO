using UnityEngine;

public class SaveData
{
    public ESceneNames SceneName { get; set; }
    public ERegions Region { get; set; }
    public int RoomIndex { get; set; }
    public Vector3 SavePointPosition { get; set; }

    public SaveData()
    {
        SceneName = ESceneNames.TitleScene;
        Region = ERegions.None;
        RoomIndex = 0;
        SavePointPosition = Vector3.zero;
    }

    public SaveData(ESceneNames sceneName, ERegions region, int roomIndex, Vector3 savePointPosition)
    {
        SceneName = sceneName;
        Region = region;
        RoomIndex = roomIndex;
        SavePointPosition = savePointPosition;
    }

    public SaveDataDTO ToDTO()
    {
        return new SaveDataDTO(SceneName, Region, RoomIndex, SavePointPosition.x, SavePointPosition.y, SavePointPosition.z);
    }
}
