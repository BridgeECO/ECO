using UnityEngine;

[CreateAssetMenu(fileName = "HiddenTerrainGimmickSO", menuName = "Scriptable Objects/Terrain Gimmick/HiddenTerrainGimmickSO")]
public class HiddenTerrainGimmickSO : TerrainGimmickBaseSO
{
    [Tooltip("스프라이트가 투명/불투명으로 전환되는 데 걸리는 시간(초).")]
    [SerializeField]
    private float _fadeDuration;

    [Tooltip("스프라이트 크기 대비 상하좌우로 확장되는 플레이어 감지 범위(블록 단위). 3이면 스프라이트 가장자리로부터 상하좌우 3칸 이내에 접근 시 투명화됩니다.")]
    [SerializeField]
    private float _detectionPadding;

    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new HiddenTerrainGimmick(ActivationType, IsInverted, _fadeDuration, _detectionPadding);
    }
}
