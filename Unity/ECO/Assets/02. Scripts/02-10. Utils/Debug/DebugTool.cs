#if UNITY_EDITOR
using UnityEngine;

// 에디터 전용 디버그 유틸. 런타임 코드가 이 클래스에 의존해서는 안 된다.
// (static 상태를 두지 않기 위해 에디터 전용 코드에 한해 Find 계열 탐색을 허용한다)
public static class DebugTool
{
    public static void ChangeCurrentRoom(Room targetRoom, Vector3 spawnPoint)
    {
        PlayerMotor playerMotor = Object.FindAnyObjectByType<PlayerMotor>();
        if (playerMotor == null)
        {
            Debug.LogWarning("[DebugTool] 씬에서 플레이어를 찾을 수 없습니다.");
            return;
        }

        Region.Instance.SetCurrentRoom(targetRoom);
        playerMotor.Teleport(spawnPoint);
    }
}
#endif
