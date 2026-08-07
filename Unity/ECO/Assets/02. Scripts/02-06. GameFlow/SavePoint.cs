using System;
using UnityEngine;
using VInspector;

/// <summary>
/// 플레이어가 통과하는 즉시 게임을 저장하는 체크포인트.
/// Room 경계와 무관하게 배치할 수 있으며, 통과할 때마다 라이프를 최대치로 회복시킨다.
/// 소속 Room은 부모 Room이 Awake에서 InitRoom으로 주입한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    public Action OnSaved;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Transform _respawnPoint;

    // 진행 순서는 Region의 Room 목록 순서로 판정한다.
    // 한 방에 세이브 포인트를 둘 이상 둘 때만 이 값으로 방 안에서의 선후를 구분한다.
    [Foldout("Settings")]
    [SerializeField]
    private int _orderInRoom = 0;

    public Room OwnerRoom { get; private set; }
    public int OrderInRoom => _orderInRoom;

    // 한 번 발동한 세이브 포인트는 비활성 상태가 되어 다시 발동하지 않는다.
    // 리스폰 라이프(2개)로 되살아난 뒤 같은 세이브 포인트를 다시 지나면
    // 최대치로 회복되어 버려 리스폰 페널티가 사라지기 때문이다.
    // Room.ResetRoom의 대상이 아니므로 사망해도 이 상태는 유지된다.
    public bool IsUsed { get; private set; }

    // 트리거 중심이 발판에서 떠 있을 수 있어 실제 부활 좌표를 따로 지정할 수 있게 한다.
    public Vector3 RespawnPosition => _respawnPoint != null ? _respawnPoint.position : transform.position;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            return;
        }

        if (IsUsed)
        {
            return;
        }

        // 통과 사실만 발행한다. 리스폰 중 재발동 억제와 저장/라이프 회복 처리는
        // RespawnManager가 구독해 판단하고, 수락 시 SetUsed를 호출한다.
        // (SavePoint가 매니저들을 직접 호출하지 않기 위한 구조)
        EventManager.Instance.BroadcastEvent(EEventType.SavePointReached, this);
    }

    public void InitRoom(Room room)
    {
        OwnerRoom = room;
    }

    /// <summary>
    /// 세이브 포인트 통과가 수락되어 사용 처리되었음을 표시한다. RespawnManager가 호출한다.
    /// </summary>
    public void SetUsed()
    {
        IsUsed = true;
        OnSaved?.Invoke();
    }

    /// <summary>
    /// 진행 순서상 other보다 뒤에 있는 세이브 포인트인지 판정한다.
    /// Region의 Room 목록 순서가 1차 기준이고, 같은 방 안에서만 OrderInRoom으로 선후를 가린다.
    /// </summary>
    public bool IsAheadOf(SavePoint other)
    {
        if (other == null)
        {
            return true;
        }

        int roomIndex = OwnerRoom != null ? OwnerRoom.Index : Room.INVALID_INDEX;
        int otherRoomIndex = other.OwnerRoom != null ? other.OwnerRoom.Index : Room.INVALID_INDEX;

        // Room 하위에 배치되지 않은 세이브 포인트는 순서를 알 수 없다.
        // 저장 자체를 막으면 진행이 불가능해지므로 이때는 갱신을 허용한다.
        if (roomIndex == Room.INVALID_INDEX || otherRoomIndex == Room.INVALID_INDEX)
        {
            return true;
        }

        if (roomIndex != otherRoomIndex)
        {
            return otherRoomIndex < roomIndex;
        }
        return other.OrderInRoom < OrderInRoom;
    }
}
