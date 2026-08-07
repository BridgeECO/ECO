using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Region 저작을 돕는 에디터 전용 도구. Region 컴포넌트의 컨텍스트 메뉴에서 실행한다.
/// </summary>
public static class RegionEditorTools
{
    private const string ROOM_NAME_PREFIX = "SN_";
    private const int UNSORTED_ROOM_ORDER = 999;

    [MenuItem("CONTEXT/Region/Auto Assign Room List")]
    private static void AutoAssignRooms(MenuCommand command)
    {
        Region region = command.context as Region;
        if (region == null)
        {
            return;
        }

        List<Room> sortedRooms = region.GetComponentsInChildren<Room>(true).OrderBy(GetRoomOrder).ToList();

        SerializedObject serializedObject = new SerializedObject(region);
        SerializedProperty roomsProperty = serializedObject.FindProperty("_rooms");
        if (roomsProperty == null)
        {
            return;
        }

        roomsProperty.ClearArray();
        for (int i = 0; i < sortedRooms.Count; i++)
        {
            roomsProperty.InsertArrayElementAtIndex(i);
            roomsProperty.GetArrayElementAtIndex(i).objectReferenceValue = sortedRooms[i];
        }
        serializedObject.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(region.gameObject.scene);
        Debug.Log($"[Region] 자식 Room {sortedRooms.Count}개를 숫자로 정렬하여 자동 할당했습니다.", region);
    }

    /// <summary>
    /// 세이브 포인트는 소속 Room을 부모에게서 주입받으므로 Room 하위 배치가 유일한 전제 조건이다.
    /// 이 전제를 런타임 전에 검사한다.
    /// </summary>
    [MenuItem("CONTEXT/Region/Validate Save Points")]
    private static void ValidateSavePoints(MenuCommand command)
    {
        Region region = command.context as Region;
        if (region == null)
        {
            return;
        }

        SavePoint[] savePoints = FindSavePointsInScene(region.gameObject.scene);
        if (savePoints.Length == 0)
        {
            Debug.LogWarning("[Region] 씬에 세이브 포인트가 하나도 없습니다. 저장은 세이브 포인트에서만 이루어집니다.", region);
            return;
        }

        int invalidCount = ReportOrphanSavePoints(savePoints, region);
        invalidCount += ReportTooCloseSavePoints(savePoints);
        invalidCount += ReportDuplicateOrders(savePoints);

        if (invalidCount == 0)
        {
            Debug.Log($"[Region] 세이브 포인트 {savePoints.Length}개 검사 완료. 문제 없습니다.", region);
        }
    }

    // 멀티 씬 편집 중에는 다른 지역의 세이브 포인트까지 잡히므로 검사 대상을 같은 씬으로 한정한다.
    // Region 하위로 좁히지 않는 이유는, Room 밖에 잘못 놓인 세이브 포인트를 잡아내는 것이 이 도구의 목적이기 때문이다.
    private static SavePoint[] FindSavePointsInScene(Scene scene)
    {
        SavePoint[] allSavePoints = Object.FindObjectsByType<SavePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<SavePoint> savePointsInScene = new List<SavePoint>();

        for (int i = 0; i < allSavePoints.Length; i++)
        {
            if (allSavePoints[i].gameObject.scene == scene)
            {
                savePointsInScene.Add(allSavePoints[i]);
            }
        }
        return savePointsInScene.ToArray();
    }

    private static int ReportOrphanSavePoints(SavePoint[] savePoints, Region region)
    {
        int invalidCount = 0;
        for (int i = 0; i < savePoints.Length; i++)
        {
            Room ownerRoom = savePoints[i].GetComponentInParent<Room>(true);
            if (ownerRoom == null)
            {
                invalidCount++;
                Debug.LogWarning($"[Region] '{savePoints[i].name}'이(가) Room 하위에 있지 않습니다. 부활 시 카메라 바운드와 BGM이 복원되지 않습니다.", savePoints[i]);
                continue;
            }

            if (!region.Rooms.Contains(ownerRoom))
            {
                invalidCount++;
                Debug.LogWarning($"[Region] '{savePoints[i].name}'의 소속 방 '{ownerRoom.name}'이(가) Region의 Room 목록에 없습니다. Auto Assign Room List를 실행하세요.", savePoints[i]);
            }
        }
        return invalidCount;
    }

    // 이어하기는 저장 좌표에서 가장 가까운 세이브 포인트를 찾으므로,
    // 허용 반경 안에 두 개가 있으면 어느 쪽으로 복원될지 저작 시점에 알 수 없다.
    private static int ReportTooCloseSavePoints(SavePoint[] savePoints)
    {
        int invalidCount = 0;
        for (int i = 0; i < savePoints.Length; i++)
        {
            for (int j = i + 1; j < savePoints.Length; j++)
            {
                float distance = Vector2.Distance(savePoints[i].RespawnPosition, savePoints[j].RespawnPosition);
                if (Region.SAVE_POINT_MATCH_DISTANCE <= distance)
                {
                    continue;
                }
                invalidCount++;
                Debug.LogWarning($"[Region] '{savePoints[i].name}'과(와) '{savePoints[j].name}'이(가) {distance:F2} 유닛 거리로 너무 가깝습니다. 이어하기 복원 지점이 모호해집니다.", savePoints[i]);
            }
        }
        return invalidCount;
    }

    // 진행 순서는 방 순서로 판정하므로, 같은 방 안에서는 Order In Room이 서로 달라야 한다.
    // 같으면 뒤쪽 세이브 포인트가 앞선 것으로 판정되지 않아 영영 갱신되지 않는다.
    private static int ReportDuplicateOrders(SavePoint[] savePoints)
    {
        int invalidCount = 0;
        for (int i = 0; i < savePoints.Length; i++)
        {
            Room room = savePoints[i].GetComponentInParent<Room>(true);
            if (room == null)
            {
                continue;
            }

            for (int j = i + 1; j < savePoints.Length; j++)
            {
                if (room != savePoints[j].GetComponentInParent<Room>(true))
                {
                    continue;
                }
                if (savePoints[i].OrderInRoom != savePoints[j].OrderInRoom)
                {
                    continue;
                }
                invalidCount++;
                Debug.LogWarning($"[Region] '{savePoints[i].name}'과(와) '{savePoints[j].name}'이(가) 같은 방 '{room.name}'에서 Order In Room {savePoints[i].OrderInRoom}을(를) 공유합니다. 뒤쪽 세이브 포인트가 갱신되지 않습니다.", savePoints[i]);
            }
        }
        return invalidCount;
    }

    private static int GetRoomOrder(Room room)
    {
        int startIndex = room.name.IndexOf(ROOM_NAME_PREFIX);
        if (startIndex == -1)
        {
            return UNSORTED_ROOM_ORDER;
        }

        startIndex += ROOM_NAME_PREFIX.Length;
        string numberText = string.Empty;
        while (startIndex < room.name.Length && char.IsDigit(room.name[startIndex]))
        {
            numberText += room.name[startIndex];
            startIndex++;
        }
        return int.TryParse(numberText, out int number) ? number : UNSORTED_ROOM_ORDER;
    }
}
