using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 상호작용 범위 안의 버튼식 후보 중 몸통에서 가장 가까운 하나(트리거 콜라이더 단위)만 골라
/// 선택, 해제를 알린다. 선택된 쪽이 작동 중이거나 입력이 막힌 동안에는 선택을 바꾸지 않는다.
/// </summary>
public class PlayerInteractTargetSelector : MonoBehaviour
{
    // 거리 제곱 차이가 이 값 이하면 같은 거리로 보고 지금 선택을 유지한다.
    // 두 대상 사이에 서 있을 때 선택이 프레임마다 오가며 표시가 깜박이는 것을 막는 부동소수 비교 여유다.
    private const float SAME_DISTANCE_TOLERANCE = 0.0001f;
    private const int INITIAL_CAPACITY = 8;

    [Foldout("Hierarchy")]
    [SerializeField]
    private BoxCollider2D _bodyCollider;

    // 같은 순번이 한 후보(대상, 그 대상의 트리거 콜라이더)다. 목록 순서가 곧 범위에 들어온 순서다.
    // 한 콜라이더에 대상이 둘이면(맵뷰 + 스위치) 두 후보가 같은 콜라이더를 가리켜 함께 선택된다.
    private readonly List<IInteractionCandidate> _candidates = new List<IInteractionCandidate>(INITIAL_CAPACITY);
    private readonly List<Collider2D> _candidateColliders = new List<Collider2D>(INITIAL_CAPACITY);
    private Collider2D _selectedCollider;

    private void FixedUpdate()
    {
        // 후보가 하나뿐이면 등록, 해제 때 이미 골라 두었으므로 매 스텝 거리를 잴 필요가 없다.
        if (_candidates.Count < 2 || InputHandler.IsInputBlocked)
        {
            return;
        }

        Reselect();
    }

    private void OnDisable()
    {
        for (int i = 0; i < _candidates.Count; i++)
        {
            _candidates[i].SetSelected(false);
        }

        _candidates.Clear();
        _candidateColliders.Clear();
        _selectedCollider = null;
    }

    public void Register(SpecialObjectBase target)
    {
        IInteractionCandidate candidate = target;
        if (_candidates.Contains(candidate) || !target.TryGetComponent(out Collider2D candidateCollider))
        {
            return;
        }

        _candidates.Add(candidate);
        _candidateColliders.Add(candidateCollider);

        // 이미 선택된 콜라이더의 대상이 뒤늦게 들어오면(맵뷰 + 스위치) 선택이 바뀌지 않아 알림이 없으므로 묶음에 함께 넣는다.
        if (candidateCollider == _selectedCollider)
        {
            candidate.SetSelected(true);
            return;
        }

        Reselect();
    }

    public void Unregister(SpecialObjectBase target)
    {
        IInteractionCandidate candidate = target;
        int index = _candidates.IndexOf(candidate);
        if (index < 0)
        {
            return;
        }

        Collider2D candidateCollider = _candidateColliders[index];
        _candidates.RemoveAt(index);
        _candidateColliders.RemoveAt(index);

        // 범위를 나간 대상은 작동 중이어도 고정을 풀고 기존 나감 처리를 받는다.
        if (candidateCollider == _selectedCollider)
        {
            candidate.SetSelected(false);
            if (!_candidateColliders.Contains(_selectedCollider))
            {
                _selectedCollider = null;
            }
        }

        Reselect();
    }

    private void Reselect()
    {
        if (IsSelectionLocked())
        {
            return;
        }

        Collider2D nearestCollider = FindNearestCollider();
        if (nearestCollider != _selectedCollider)
        {
            SetSelectedCollider(nearestCollider);
        }
    }

    // 입력이 막힌 동안(NPC 대화 등)에 선택이 바뀌면 나감 처리로 진행 중인 대화가 끊기므로 지금 선택을 지킨다.
    private bool IsSelectionLocked()
    {
        if (_selectedCollider == null)
        {
            return false;
        }

        return InputHandler.IsInputBlocked || IsSelectionEngaged();
    }

    private bool IsSelectionEngaged()
    {
        for (int i = 0; i < _candidates.Count; i++)
        {
            if (_candidateColliders[i] == _selectedCollider && _candidates[i].IsEngaged)
            {
                return true;
            }
        }

        return false;
    }

    // 몸통 가운데에서 각 후보 콜라이더 위 가장 가까운 점까지의 거리로 고른다(가운데가 콜라이더 안이면 0).
    // 같은 거리면 지금 선택을, 지금 선택이 없으면 먼저 들어온 쪽을 남긴다.
    private Collider2D FindNearestCollider()
    {
        Vector2 bodyCenter = _bodyCollider.bounds.center;
        Collider2D nearestCollider = null;
        float nearestSqrDistance = float.MaxValue;
        float selectedSqrDistance = float.MaxValue;

        for (int i = 0; i < _candidateColliders.Count; i++)
        {
            Collider2D candidateCollider = _candidateColliders[i];
            float sqrDistance = (candidateCollider.ClosestPoint(bodyCenter) - bodyCenter).sqrMagnitude;

            if (candidateCollider == _selectedCollider)
            {
                selectedSqrDistance = sqrDistance;
            }

            if (sqrDistance + SAME_DISTANCE_TOLERANCE < nearestSqrDistance)
            {
                nearestCollider = candidateCollider;
                nearestSqrDistance = sqrDistance;
            }
        }

        if (_selectedCollider != null && selectedSqrDistance - nearestSqrDistance <= SAME_DISTANCE_TOLERANCE)
        {
            return _selectedCollider;
        }

        return nearestCollider;
    }

    // 옛 대상의 F 구독을 먼저 끊은 뒤 새 대상을 구독시켜, 한 프레임에 둘이 F를 받는 틈을 없앤다.
    private void SetSelectedCollider(Collider2D nextCollider)
    {
        Collider2D previousCollider = _selectedCollider;
        _selectedCollider = nextCollider;
        NotifyCandidatesWith(previousCollider, false);
        NotifyCandidatesWith(nextCollider, true);
    }

    private void NotifyCandidatesWith(Collider2D candidateCollider, bool isSelected)
    {
        if (candidateCollider == null)
        {
            return;
        }

        for (int i = 0; i < _candidates.Count; i++)
        {
            if (_candidateColliders[i] == candidateCollider)
            {
                _candidates[i].SetSelected(isSelected);
            }
        }
    }
}
