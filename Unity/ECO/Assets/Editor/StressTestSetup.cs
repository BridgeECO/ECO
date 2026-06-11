using UnityEngine;
using UnityEditor;
using System.Linq;

public class StressTestSetup
{
    [MenuItem("Tools/Test/Setup Stress Test (Settings Popup)")]
    public static void SetupStressTest()
    {
        // 씬 내에서 기존 테스트용 팝업 찾기 (비활성화 상태인 것도 포함, 프리팹 에셋 제외)
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        var uniTaskObj = allObjects.FirstOrDefault(g => g.name == "P_UI_Popup_Settings" && g.scene.isLoaded);
        var animatorObj = allObjects.FirstOrDefault(g => g.name == "P_UI_Popup_Settings_Animator" && g.scene.isLoaded);

        if (uniTaskObj == null)
        {
            Debug.LogError("현재 씬에서 'P_UI_Popup_Settings' 오브젝트를 찾을 수 없습니다. (비활성화 상태라도 씬에 있어야 합니다)");
            return;
        }
        if (animatorObj == null)
        {
            Debug.LogError("현재 씬에서 'P_UI_Popup_Settings_Animator' 오브젝트를 찾을 수 없습니다.");
            return;
        }

        var parent = uniTaskObj.transform.parent;

        // 1. UniTask 그룹 생성 및 100개 복사
        GameObject utGroup = new GameObject("=== [ UniTask (100) ] ===");
        utGroup.transform.SetParent(parent, false);
        
        // 원본 이동 및 99개 복제
        uniTaskObj.transform.SetParent(utGroup.transform, false);
        for (int i = 0; i < 99; i++)
        {
            var clone = PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(uniTaskObj)) as GameObject;
            if (clone != null)
            {
                clone.transform.SetParent(utGroup.transform, false);
                clone.name = "P_UI_Popup_Settings_Clone";
            }
            else
            {
                var fallbackClone = Object.Instantiate(uniTaskObj, utGroup.transform);
                fallbackClone.name = "P_UI_Popup_Settings_Clone";
            }
        }

        // 2. Animator 그룹 생성 및 100개 복사
        GameObject animGroup = new GameObject("=== [ Animator (100) ] ===");
        animGroup.transform.SetParent(parent, false);
        
        // 원본 이동 및 99개 복제
        animatorObj.transform.SetParent(animGroup.transform, false);
        for (int i = 0; i < 99; i++)
        {
            var clone = PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(animatorObj)) as GameObject;
            if (clone != null)
            {
                clone.transform.SetParent(animGroup.transform, false);
                clone.name = "P_UI_Popup_Settings_Animator_Clone";
            }
            else
            {
                var fallbackClone = Object.Instantiate(animatorObj, animGroup.transform);
                fallbackClone.name = "P_UI_Popup_Settings_Animator_Clone";
            }
        }

        // 초기 상태: UniTask 활성화, Animator 비활성화
        utGroup.SetActive(false);
        animGroup.SetActive(true);

        Debug.Log("✅ 설정 팝업 100개 복사 완료! 플레이 모드로 들어가서 각각 그룹의 Active를 켜/끄며 프로파일러를 확인해보세요.");
    }
}
