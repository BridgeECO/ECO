using UnityEngine;

public class SaveData
{
    public ERegions Region { get; set; }
    public int RoomIndex { get; set; }
    public Vector3 SavePointPosition { get; set; }

    public SaveData()
    {
        Region = ERegions.None;
        RoomIndex = -1;
        SavePointPosition = Vector3.zero;
    }

    public SaveData(ERegions region, int roomIndex, Vector3 savePointPosition)
    {
        Region = region;
        RoomIndex = roomIndex;
        SavePointPosition = savePointPosition;
    }

    public SaveDataDTO ToDTO()
    {
        return new SaveDataDTO(Region, RoomIndex, SavePointPosition.x, SavePointPosition.y, SavePointPosition.z);
    }
}
