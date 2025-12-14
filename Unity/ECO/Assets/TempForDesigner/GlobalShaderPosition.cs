using UnityEngine;

public class GlobalShaderPosition : MonoBehaviour
{
    // 셰이더 그래프에서 만든 프로퍼티 이름과 똑같아야 합니다.
    // 셰이더 그래프 Blackboard에서 Reference 이름을 확인하세요 (보통 _PlayerPos)
    private int playerPosID = Shader.PropertyToID("_PlayerPos");

    void Update()
    {
        // 내(캐릭터) 위치를 모든 셰이더가 알 수 있는 '전역 변수'로 쏘아줍니다.
        Shader.SetGlobalVector(playerPosID, transform.position);
        Debug.Log(Shader.GetGlobalVector("_PlayerPos"));
    }
}