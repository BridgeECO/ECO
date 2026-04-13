public class SaveData
{
    public ERegions Region { get; set; }
    public int RoomIndex { get; set; }

    public SaveData()
    {
        Region = ERegions.None;
        RoomIndex = -1;
    }

    public SaveData(ERegions region, int roomIndex)
    {
        Region = region;
        RoomIndex = roomIndex;
    }

    public SaveDataDTO ToDTO()
    {
        return new SaveDataDTO(Region, RoomIndex);
    }
}
