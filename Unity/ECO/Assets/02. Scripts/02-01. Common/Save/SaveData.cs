public class SaveData
{
    public ERegions Region { get; set; }
    public int RoomIndex { get; set; }

    public SaveData()
    {
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
