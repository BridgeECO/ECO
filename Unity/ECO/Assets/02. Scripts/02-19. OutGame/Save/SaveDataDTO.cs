using Newtonsoft.Json;
using UnityEngine;

public class SaveDataDTO
{
    public readonly ESceneNames SceneName;
    public readonly ERegions Region;

    // Vector3는 JSON 직렬화 시 순환 참조 문제가 생길 수 있어 float 3개로 분리 저장
    public readonly float SavePointX;
    public readonly float SavePointY;
    public readonly float SavePointZ;

    [JsonIgnore]
    public Vector3 SavePointPosition => new Vector3(SavePointX, SavePointY, SavePointZ);

    [JsonConstructor]
    public SaveDataDTO(ESceneNames sceneName, ERegions region, float savePointX, float savePointY, float savePointZ)
    {
        SceneName = sceneName;
        Region = region;
        SavePointX = savePointX;
        SavePointY = savePointY;
        SavePointZ = savePointZ;
    }
}
