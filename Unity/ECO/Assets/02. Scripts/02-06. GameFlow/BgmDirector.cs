using UnityEngine;

/// <summary>
/// BGM 전환 정책의 단일 소유자. PersistentScene에 상주한다.
/// Region/RespawnManager/SceneTransitionManager에 흩어져 있던 BGM 하드코딩을
/// 이벤트 구독 방식으로 모아, 각 시스템이 SoundManager를 직접 호출하지 않게 한다.
/// 보스방 BGM은 보스 시스템이 직접 제어하므로 여기서는 보스방에서 무음을 유지한다.
/// </summary>
public class BgmDirector : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Instance.AddEventListener<Room>(EEventType.RoomChanged, OnRoomChanged);
        EventManager.Instance.AddEventListener<Room>(EEventType.RespawnStateRestored, OnRespawnStateRestored);
        EventManager.Instance.AddEventListener<ESceneNames>(EEventType.SceneTransitionStarted, OnSceneTransitionStarted);
    }

    private void OnDisable()
    {
        RemoveEventListeners();
    }

    private void RemoveEventListeners()
    {
        if (!EventManager.HasInstance)
        {
            return;
        }
        EventManager.Instance.RemoveEventListener<Room>(EEventType.RoomChanged, OnRoomChanged);
        EventManager.Instance.RemoveEventListener<Room>(EEventType.RespawnStateRestored, OnRespawnStateRestored);
        EventManager.Instance.RemoveEventListener<ESceneNames>(EEventType.SceneTransitionStarted, OnSceneTransitionStarted);
    }

    private void OnRoomChanged(Room newRoom)
    {
        if (!SoundManager.HasInstance || newRoom == null)
        {
            return;
        }

        if (newRoom.RoomType == ERoomType.Boss)
        {
            SoundManager.Instance.StopBgm();
            return;
        }
        SoundManager.Instance.PlayBgm(EBgmType.SyrNormal);
    }

    // 리스폰 방 복원 직후(페이드인 직전) 호출된다.
    // 동일 트랙도 처음부터 다시 재생되도록 StopBgm으로 상태를 초기화한 뒤 재생한다.
    private void OnRespawnStateRestored(Room saveRoom)
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }

        SoundManager.Instance.StopBgm();

        // 보스방은 보스가 추격 시작 시 BGM을 켜므로 무음 유지
        if (saveRoom != null && saveRoom.RoomType == ERoomType.Boss)
        {
            return;
        }
        SoundManager.Instance.PlayBgm(EBgmType.SyrNormal);
    }

    // 씬 전환 시작 시(페이드아웃 전) 대상 씬에 맞는 BGM으로 크로스페이드한다.
    private void OnSceneTransitionStarted(ESceneNames targetSceneName)
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }

        if (targetSceneName == ESceneNames.TitleScene)
        {
            SoundManager.Instance.PlayBgm(EBgmType.Title);
            return;
        }
        SoundManager.Instance.PlayBgm(EBgmType.SyrNormal);
    }
}
