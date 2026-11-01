using UnityEditor;
using UnityEngine;

/// <summary>
/// 에너지원이 생기기 전에 만든 룸을 새 구조로 옮기는 Room 컨텍스트 메뉴.
/// 선의 경로와 판정 거리는 그대로 두므로 코어와 지형이 켜지고 꺼지는 시점이 변환 전과 같다.
/// 다시 실행해도 이미 옮긴 연결은 건너뛴다.
/// </summary>
public static class LegacyEnergyConverter
{
    [MenuItem("CONTEXT/Room/Convert Legacy Energy")]
    private static void ConvertRoom(MenuCommand command)
    {
        Room room = command.context as Room;
        if (room == null)
        {
            return;
        }

        if (IsLockedByOuterPrefab(room))
        {
            return;
        }

        Undo.SetCurrentGroupName($"Convert Legacy Energy ({room.name})");
        int undoGroup = Undo.GetCurrentGroup();
        int switchCount = LegacySwitchConverter.Convert(room);
        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"[Energy] '{room.name}' 변환 완료. 스위치 {switchCount}개를 에너지원 제어로 옮겼습니다.", room);
    }

    // 룸 프리팹 안의 스위치를 씬에서 고치면 변경이 룸 프리팹이 아니라 씬의 오버라이드로 남는다.
    // 씬에 직접 놓은 스위치(추가한 오브젝트)만 그 자리에서 고치고, 나머지는 프리팹을 열어서 실행하게 한다.
    private static bool IsLockedByOuterPrefab(Room room)
    {
        EnergySwitch[] switches = room.GetComponentsInChildren<EnergySwitch>(true);
        for (int i = 0; i < switches.Length; i++)
        {
            GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(switches[i].gameObject);
            if (instanceRoot == null || instanceRoot.transform.parent == null)
            {
                continue;
            }

            GameObject parent = instanceRoot.transform.parent.gameObject;
            if (!PrefabUtility.IsPartOfPrefabInstance(parent) || PrefabUtility.IsAddedGameObjectOverride(instanceRoot))
            {
                continue;
            }

            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(parent);
            Debug.LogWarning($"[Energy] '{switches[i].name}'은(는) 프리팹 '{prefabPath}' 안에 있습니다. 그 프리팹을 프리팹 모드로 열고 Room에서 다시 실행하세요.", switches[i]);
            return true;
        }
        return false;
    }
}
