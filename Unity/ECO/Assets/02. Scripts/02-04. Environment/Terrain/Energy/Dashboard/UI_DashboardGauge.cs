using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 계기판 막대에 에너지 펄스 구간을 그린다. 막대 아래 끝이 에너지원, 위 끝이 선의 가장 먼 끝이다.
/// 구간은 펄스 수만큼 풀에서 꺼내고, 펄스가 움직이면 이미 꺼낸 구간의 자리와 길이만 바꾼다.
/// 구간은 풀 루트 아래에 둔 채 월드 좌표로 맞춘다. 계기판 아래로 옮기면 리전 언로드 때 계기판과 함께 파괴되어
/// 풀이 파괴된 오브젝트를 다시 꺼내게 되므로, 구간마다 월드 스페이스 캔버스를 두고 따로 그린다.
/// </summary>
public class UI_DashboardGauge : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Tooltip("구간을 그릴 막대 영역입니다. 피벗은 아래 가운데입니다.")]
    [SerializeField]
    private RectTransform _barRect;

    [Tooltip("계기판 캔버스입니다. 구간은 이 캔버스 바로 위 순서로 그립니다.")]
    [SerializeField]
    private Canvas _canvas;

    [Tooltip("코어침을 담는 캔버스입니다. 구간보다 위에 그려지도록 순서를 맞춥니다.")]
    [SerializeField]
    private Canvas _overlayCanvas;

    private List<RectTransform> _segments = new List<RectTransform>();

    private void Awake()
    {
        _overlayCanvas.overrideSorting = true;
        _overlayCanvas.sortingLayerID = _canvas.sortingLayerID;
        _overlayCanvas.sortingOrder = _canvas.sortingOrder + 2;
    }

    private void OnDisable()
    {
        ReleaseSegments();
    }

    // 펄스가 생기거나 사라질 때만 풀을 쓴다.
    public void SetPulseCount(int count)
    {
        while (_segments.Count < count)
        {
            if (!TryAcquireSegment())
            {
                return;
            }
        }

        while (count < _segments.Count)
        {
            ReleaseLastSegment();
        }
    }

    // 비율은 막대 아래 끝이 0, 위 끝이 1이다.
    public void SetPulse(int index, float tailRatio, float headRatio)
    {
        if (_segments.Count <= index)
        {
            return;
        }

        Rect bar = _barRect.rect;
        RectTransform segment = _segments[index];
        segment.position = _barRect.TransformPoint(new Vector3(bar.center.x, bar.yMin + bar.height * tailRatio, 0f));
        segment.sizeDelta = new Vector2(bar.width, bar.height * (headRatio - tailRatio));
    }

    public void ReleaseSegments()
    {
        while (0 < _segments.Count)
        {
            ReleaseLastSegment();
        }
    }

    private bool TryAcquireSegment()
    {
        // Managers 프리팹이 없는 씬(프리팹 단독 테스트 등)에서는 막대 없이 침만 동작한다.
        if (!PoolManager.HasInstance)
        {
            return false;
        }

        GameObject segment = PoolManager.Instance.Pop(EPoolable.DashboardGaugeSegment);
        if (segment == null)
        {
            return false;
        }

        RectTransform segmentRect = segment.GetComponent<RectTransform>();
        segmentRect.rotation = _barRect.rotation;
        segmentRect.localScale = _barRect.lossyScale;

        Canvas segmentCanvas = segment.GetComponent<Canvas>();
        segmentCanvas.sortingLayerID = _canvas.sortingLayerID;
        segmentCanvas.sortingOrder = _canvas.sortingOrder + 1;

        _segments.Add(segmentRect);
        return true;
    }

    private void ReleaseLastSegment()
    {
        int lastIndex = _segments.Count - 1;
        RectTransform segment = _segments[lastIndex];
        _segments.RemoveAt(lastIndex);

        // 앱 종료 중에는 매니저나 구간이 먼저 파괴되어 반납할 곳이 없을 수 있다.
        if (PoolManager.HasInstance && segment != null)
        {
            PoolManager.Instance.Push(EPoolable.DashboardGaugeSegment, segment.gameObject);
        }
    }
}
