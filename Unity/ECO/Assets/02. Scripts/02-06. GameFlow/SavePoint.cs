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

    // 트리거 중심이 발판에서 떠 있을 수 있어 실제 부활 좌표를 따로 지정할 수 있게 한다.
    public Vector3 RespawnPosition => _respawnPoint != null ? _respawnPoint.position : transform.position;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            return;
        }

        // 리스폰은 플레이어를 이 트리거 위로 텔레포트시키므로 페이드 도중 재발동한다.
        // 막지 않으면 리스폰 라이프(2개)가 즉시 최대치로 덮어써져 하트 연출이 튄다.
        if (RespawnManager.Instance == null || RespawnManager.Instance.IsRespawning)
        {
            return;
        }

        RespawnManager.Instance.SetSavePoint(this);
        LifeManager.Instance.SetLifeToMax();
        OnSaved?.Invoke();
    }

    public void InitRoom(Room room)
    {
        OwnerRoom = room;
    }

    /// <summary>
    /// 진행 순서상 other보다 뒤에 있는 세이브 포인트인지 판정한다.
    /// Region의 Room 목록 순서가 1차 기준이고, 같은 방 안에서만 OrderInRoom으로 선후를 가린다.
    /// </summary>
    public bool IsAheadOf(SavePoint other)
    {
        if (other == null || Region.Instance == null)
        {
            return true;
        }

        int roomIndex = Region.Instance.GetRoomIndex(OwnerRoom);
        int otherRoomIndex = Region.Instance.GetRoomIndex(other.OwnerRoom);

        // Room 하위에 배치되지 않은 세이브 포인트는 순서를 알 수 없다.
        // 저장 자체를 막으면 진행이 불가능해지므로 이때는 갱신을 허용한다.
        if (roomIndex == Region.INVALID_ROOM_INDEX || otherRoomIndex == Region.INVALID_ROOM_INDEX)
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
