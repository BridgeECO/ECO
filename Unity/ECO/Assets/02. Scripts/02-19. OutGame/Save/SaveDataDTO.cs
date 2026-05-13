using Newtonsoft.Json;

public class SaveDataDTO
{
    public readonly ERegions Region;
    public readonly int RoomIndex;

    [JsonConstructor]
    public SaveDataDTO(ERegions region, int roomIndex)
    {
        Region = region;
        RoomIndex = roomIndex;
    }
}
