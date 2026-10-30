using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 계기판. 연결한 에너지선의 상태를 막대(펄스 구간), 코어침, 에너지원침으로 보여 준다(기획서 "계기판").
/// 막대는 아래가 에너지원이고 전체 길이가 선의 가장 긴 경로 거리에 대응한다. 코어침은 코어가 켜지는 거리의 비율 자리에 있어
/// 막대 끝이 코어침에 닿는 프레임에 코어가 켜진다. 이 클래스는 게임 상태를 읽어 보여 줄 값을 정하고, 그리기는 UI_ 클래스가 맡는다.
/// </summary>
public class EnergyDashboard : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private EnergyLine _line;

    [SerializeField]
    private UI_DashboardGauge _gauge;

    [SerializeField]
    private UI_SourceNeedle _sourceNeedle;

    [Tooltip("코어침을 두는 영역입니다. 막대와 같은 높이이고, 코어침은 비율에 맞춘 앵커로 자리를 잡습니다.")]
    [SerializeField]
    private RectTransform _needleArea;

    [Tooltip("선이 지나는 코어마다 하나씩, 선의 노드 순서대로 둡니다. '계기판 맞추기' 버튼이 만들고 지웁니다.")]
    [SerializeField]
    private List<UI_CoreNeedle> _coreNeedles = new List<UI_CoreNeedle>();

    [Foldout("Project")]
    [Tooltip("'계기판 맞추기'가 코어침을 새로 만들 때 쓰는 프리팹입니다.")]
    [SerializeField]
    private UI_CoreNeedle _coreNeedlePrefab;

    [Foldout("Settings")]
    [Tooltip("막대 길이(월드 단위)입니다. 계기판마다 다르게 정할 수 있고, 바꾸면 계기판 높이와 코어침 자리가 함께 맞춰집니다.")]
    [Min(0.5f)]
    [SerializeField]
    private float _barLength = 4f;

    private DashboardGaugeBinding _gaugeBinding;
    private List<DashboardCoreBinding> _coreBindings = new List<DashboardCoreBinding>();
    private bool _isInitialized;

    public EnergyLine Line => _line;
    public IReadOnlyList<UI_CoreNeedle> CoreNeedles => _coreNeedles;
    public RectTransform NeedleArea => _needleArea;
    public UI_CoreNeedle CoreNeedlePrefab => _coreNeedlePrefab;
    public float BarLength => _barLength;

    // 처음 켜질 때는 선의 그래프가 만들어지는 EnergyLine.Awake 뒤인 Start에서 묶는다. 다시 켜질 때는 OnEnable이 묶는다.
    private void OnEnable()
    {
        if (!_isInitialized)
        {
            return;
        }

        SubscribeEvents();
        RefreshAll();
    }

    private void Start()
    {
        _gaugeBinding = new DashboardGaugeBinding(_line, _gauge);
        InitCoreBindings();
        _isInitialized = true;
        SubscribeEvents();
        RefreshAll();
    }

    // 펄스는 EnergyLine.Update에서, 지형은 물리 스텝에서 움직이므로 같은 프레임의 결과를 그리도록 LateUpdate에서 갱신한다.
    private void LateUpdate()
    {
        _gaugeBinding.Refresh();
        for (int i = 0; i < _coreBindings.Count; i++)
        {
            _coreBindings[i].RefreshIfMoving();
        }
    }

    private void OnDisable()
    {
        if (!_isInitialized)
        {
            return;
        }

        UnsubscribeEvents();
        _gaugeBinding.ResetDrawnState();
    }

#if UNITY_EDITOR
    // 막대 길이를 바꾸면 계기판 높이와 코어침을 바로 맞춘다. OnValidate 안에서는 계층을 바꿀 수 없어 한 틱 미룬다.
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall -= SyncLayout;
        UnityEditor.EditorApplication.delayCall += SyncLayout;
    }
#endif

    // 코어침은 선의 노드 순서대로 코어와 짝을 짓는다. 선을 고친 뒤 맞추지 않았으면 앞에서부터 맞는 개수만 묶는다.
    private void InitCoreBindings()
    {
        List<EnergyPathNode> coreNodes = new List<EnergyPathNode>();
        _line.Graph.CollectCoreNodes(coreNodes);
        if (coreNodes.Count != _coreNeedles.Count)
        {
            Debug.LogWarning($"[EnergyDashboard] {name}의 코어침은 {_coreNeedles.Count}개인데 선이 지나는 코어는 {coreNodes.Count}개입니다. '계기판 맞추기' 버튼을 누르세요.", this);
        }

        _coreBindings.Clear();
        int count = Mathf.Min(coreNodes.Count, _coreNeedles.Count);
        for (int i = 0; i < count; i++)
        {
            _coreBindings.Add(new DashboardCoreBinding(coreNodes[i].EnergyCore, _coreNeedles[i]));
        }
    }

    private void SubscribeEvents()
    {
        _line.OnSupplyChanged += HandleSupplyChanged;
        for (int i = 0; i < _coreBindings.Count; i++)
        {
            _coreBindings[i].Subscribe();
        }
    }

    private void UnsubscribeEvents()
    {
        _line.OnSupplyChanged -= HandleSupplyChanged;
        for (int i = 0; i < _coreBindings.Count; i++)
        {
            _coreBindings[i].Unsubscribe();
        }
    }

    private void RefreshAll()
    {
        _sourceNeedle.SetLit(_line.IsSupplied);
        for (int i = 0; i < _coreBindings.Count; i++)
        {
            _coreBindings[i].Refresh();
        }
        _gaugeBinding.Refresh();
    }

    private void HandleSupplyChanged(bool isSupplied)
    {
        _sourceNeedle.SetLit(isSupplied);
    }

#if UNITY_EDITOR
    [Button("계기판 맞추기")]
    private void SyncLayout()
    {
        EnergyDashboardLayout.Sync(this);
    }
#endif
}
