using UnityEngine;

public class SaveData
{
    public ESceneNames SceneName { get; set; }
    public ERegions Region { get; set; }
    public Vector3 SavePointPosition { get; set; }

    public SaveData()
    {
        SceneName = ESceneNames.TitleScene;
        Region = ERegions.None;
        SavePointPosition = Vector3.zero;
    }

    public SaveData(ESceneNames sceneName, ERegions region, Vector3 savePointPosition)
    {
        SceneName = sceneName;
        Region = region;
        SavePointPosition = savePointPosition;
    }

    public SaveDataDTO ToDTO()
    {
        return new SaveDataDTO(SceneName, Region, SavePointPosition.x, SavePointPosition.y, SavePointPosition.z);
    }
}
