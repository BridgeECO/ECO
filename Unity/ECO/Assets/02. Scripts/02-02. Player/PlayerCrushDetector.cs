using UnityEngine;
using VInspector;

/// <summary>
/// 지형이 몸통 안으로 들어와 빠져나가지 못하면 지형 사이에 끼인 것으로 보고 즉사시킨다. 움직이는 지형이 벽이나 천장으로
/// 밀어붙이는 경우와, 몸 위에서 켜진 지형이 빠질 곳 없이 누르는 경우다. 켜진 지형이라도 빈자리가 있어
/// 물리 엔진이 밀어내는 중이면 끼임으로 보지 않는다.
/// </summary>
// 다른 스크립트의 FixedUpdate보다 먼저 돌아 직전 물리 스텝이 정리한 상태만 본다. 모서리 보정은 FixedUpdate에서
// 강체 위치를 직접 옮기는데, 좁은 통로에서는 몸통을 옆벽 안으로 0.1가량 넣었다가 이어지는 물리 스텝이 밀어낸다.
// 그 뒤에 검사하면 이 순간을 끼임으로 오판한다.
[DefaultExecutionOrder(-1)]
public class PlayerCrushDetector : MonoBehaviour
{
    // 몸통 안에 함께 들어올 수 있는 지형 수. 미는 지형과 벽, 타고 있는 지형과 천장처럼 둘이 보통이라 여유를 둔다.
    private const int OVERLAP_CAPACITY = 4;

    // 겹친 깊이가 한 스텝에 이만큼 넘게 줄면 물리 엔진이 몸을 밀어내 빠져나가는 중으로 본다. 측정에서 밀려나는 겹침은
    // 한 스텝에 0.37 이상 줄었고, 빠질 곳 없이 끼인 겹침은 처음 한 번 0.013 이하로 줄고 그대로였다.
    private const float ESCAPE_DEPTH_PER_STEP = 0.1f;

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

    // 빠져나가지 못하는 겹침이 이만큼의 물리 스텝 동안 이어지면 즉사시킨다. 처음 겹친 스텝에서는 다음 스텝에 밀려날지
    // 알 수 없으므로 2 이상이어야 한다. 움직이는 지형에 끼면 처음 겹친 다음 스텝에 겹침이 더 깊어져 바로 걸린다.
    [SerializeField]
    [Min(2)]
    private int _requiredOverlapSteps = 2;

    // 매 물리 스텝 조회하므로 리스트 대신 결과 개수가 고정되는 배열을 미리 할당해 재사용한다.
    private readonly Collider2D[] _overlapResults = new Collider2D[OVERLAP_CAPACITY];
    private ContactFilter2D _terrainFilter;
    private int _overlapSteps;
    private float _previousDepth;

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
        ResetOverlap();
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
            ResetOverlap();
            return;
        }

        float depth = MeasureDeepestOverlap(out Collider2D crushingTerrain);
        if (depth <= 0f)
        {
            ResetOverlap();
            return;
        }

        bool isEscaping = depth < _previousDepth - ESCAPE_DEPTH_PER_STEP;
        _previousDepth = depth;
        _overlapSteps = isEscaping ? 0 : _overlapSteps + 1;
        if (_overlapSteps < _requiredOverlapSteps)
        {
            return;
        }

        ResetOverlap();
        _playerLife.InstantKill(EPlayerDeathCause.Crush, _bodyCollider, crushingTerrain);
    }

    private void ResetOverlap()
    {
        _overlapSteps = 0;
        _previousDepth = 0f;
    }

    private bool IsRespawning()
    {
        return RespawnManager.HasInstance && RespawnManager.Instance.IsRespawning;
    }

    // 몸통 안으로 들어온 지형 가운데 가장 깊이 겹친 지형과 그 깊이를 구한다. 들어온 지형이 없으면 0이다.
    // 깊이는 두 콜라이더를 떼어 놓는 데 필요한 최소 거리라, 미는 쪽과 받치는 쪽 중 어느 지형이 더 파고들어도 커진다.
    private float MeasureDeepestOverlap(out Collider2D deepestTerrain)
    {
        deepestTerrain = null;
        Bounds body = _bodyCollider.bounds;
        Vector2 innerSize = new Vector2(body.size.x - _insetMargin * 2f, body.size.y - _insetMargin * 2f);
        int count = Physics2D.OverlapBox(body.center, innerSize, 0f, _terrainFilter, _overlapResults);

        float deepest = 0f;
        for (int i = 0; i < count; i++)
        {
            float depth = -Physics2D.Distance(_bodyCollider, _overlapResults[i]).distance;
            if (deepest < depth)
            {
                deepest = depth;
                deepestTerrain = _overlapResults[i];
            }
        }
        return deepest;
    }
}
