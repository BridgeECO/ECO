using UnityEngine;
using VInspector;

/// <summary>
/// 움직이는 지형이 플레이어를 벽이나 천장으로 밀어붙이거나 몸 위에서 지형이 켜져 지형이 몸통 안으로 들어오면
/// 지형 사이에 끼인 것으로 보고 즉사시킨다.
/// </summary>
// 다른 스크립트의 FixedUpdate보다 먼저 돌아 직전 물리 스텝이 정리한 상태만 본다. 모서리 보정은 FixedUpdate에서
// 강체 위치를 직접 옮기는데, 좁은 통로에서는 몸통을 옆벽 안으로 0.1가량 넣었다가 이어지는 물리 스텝이 밀어낸다.
// 그 뒤에 검사하면 이 순간을 끼임으로 오판한다.
[DefaultExecutionOrder(-1)]
public class PlayerCrushDetector : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Collider2D _bodyCollider;

    [SerializeField]
    private PlayerLife _playerLife;

    [Foldout("Settings")]
    [SerializeField]
    private LayerMask _terrainLayer;

    // 몸통 가장자리에서 이만큼 안쪽으로 줄인 상자에 지형이 닿으면 끼인 것으로 본다. OverlapBox는 두 콜라이더의
    // 접촉 여유(defaultContactOffset의 2배, 0.02)만큼 떨어진 지형까지 잡으므로, 0.02면 몸통 경계 안으로 실제로
    // 들어온 지형만 잡힌다. 벽·천장에 기대거나 좁은 통로에 꽉 끼어 지나갈 때는 물리 엔진이 이 여유 안에서 살짝
    // 떨어뜨려 두므로 걸리지 않고, 0.01로 줄이면 걸린다.
    [SerializeField]
    private float _insetMargin = 0.02f;

    // 지형이 몸통 안에 이만큼의 물리 스텝 동안 이어서 있어야 즉사시킨다. 몸을 덮으며 켜진 지형은 바로 다음 물리 스텝에
    // 플레이어를 옆으로 밀어내 한 스텝만 겹쳐 보이므로, 2 이상으로 올리면 이 경우를 놓친다.
    // 착지·대시는 강체가 연속 충돌 검사(Continuous)라 몸통 안으로 들어오지 않아 거를 필요가 없다.
    [SerializeField]
    [Min(1)]
    private int _requiredOverlapSteps = 1;

    // 끼였는지와 피격 이펙트를 띄울 지형 하나만 알면 되므로 결과 버퍼는 1칸이다.
    // 매 물리 스텝 조회하므로 리스트 대신 결과 개수가 고정되는 배열을 미리 할당해 재사용한다.
    private readonly Collider2D[] _overlapResults = new Collider2D[1];
    private ContactFilter2D _terrainFilter;
    private int _overlapSteps;

    private void Awake()
    {
        InitTerrainFilter();
    }

    private void FixedUpdate()
    {
        UpdateCrushDetection();
    }

    private void OnDisable()
    {
        _overlapSteps = 0;
    }

    private void InitTerrainFilter()
    {
        // 일방향 발판(Platform 레이어)은 아래에서 뚫고 올라오는 몸통과 겹치므로 Terrain 레이어만 본다.
        _terrainFilter.SetLayerMask(_terrainLayer);
        // 프로젝트 설정에서 쿼리가 트리거도 잡으므로(queriesHitTriggers) 켜진 킬 기믹·가짜 지형 같은 트리거를 필터에서 뺀다.
        _terrainFilter.useTriggers = false;
    }

    private void UpdateCrushDetection()
    {
        // 사망부터 부활까지는 리스폰 텔레포트·방 초기화로 지형과 잠깐 겹칠 수 있고, 이미 시작된 사망을 다시 셀 이유도 없다.
        if (IsRespawning())
        {
            _overlapSteps = 0;
            return;
        }

        if (!TryFindCrushingTerrain(out Collider2D crushingTerrain))
        {
            _overlapSteps = 0;
            return;
        }

        _overlapSteps++;
        if (_overlapSteps < _requiredOverlapSteps)
        {
            return;
        }

        _overlapSteps = 0;
        _playerLife.InstantKill(EPlayerDeathCause.Crush, _bodyCollider, crushingTerrain);
    }

    private bool IsRespawning()
    {
        return RespawnManager.HasInstance && RespawnManager.Instance.IsRespawning;
    }

    private bool TryFindCrushingTerrain(out Collider2D crushingTerrain)
    {
        Bounds body = _bodyCollider.bounds;
        Vector2 innerSize = new Vector2(body.size.x - _insetMargin * 2f, body.size.y - _insetMargin * 2f);
        int count = Physics2D.OverlapBox(body.center, innerSize, 0f, _terrainFilter, _overlapResults);
        crushingTerrain = _overlapResults[0];
        return 0 < count;
    }
}
