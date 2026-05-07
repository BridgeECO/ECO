using System.Collections.Generic;
using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    private struct ObjectState
    {
        public GameObject Obj;
        public Vector3 Pos;
        public Quaternion Rot;
    }

    private List<ObjectState> _initialStates = new();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            _initialStates.Add(new ObjectState
            {
                Obj = child.gameObject,
                Pos = child.position,
                Rot = child.rotation
            });
        }
    }

    public void ResetRoom()
    {
        foreach (var state in _initialStates)
        {
            state.Obj.transform.position = state.Pos;
            state.Obj.transform.rotation = state.Rot;
            state.Obj.SetActive(true);
        }
    }

    //플레이어가 들어오고 나간것을 체크해서 playerinput설정
    //대쉬를 가지고있는지? -> 대쉬있으면 대쉬 끄기 -> 클리어 후 대쉬 키기
}
