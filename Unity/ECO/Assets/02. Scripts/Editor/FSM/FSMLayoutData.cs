using System.Collections.Generic;
using UnityEngine;

namespace ECOEditor
{
    /// <summary>
    /// FSM 노드 좌표/뷰 설정 저장용 ScriptableObject (에셋으로 보존)
    /// </summary>
    public class FSMLayoutData : ScriptableObject
    {
        [System.Serializable]
        public class NodePos
        {
            public string stateName;
            public Vector2 position;
        }

        [Header("Owner")]
        public GameObject fsmOwner;

        [Header("Nodes")]
        public List<NodePos> nodePositions = new List<NodePos>();

        public bool TryGet(string stateName, out Vector2 position)
        {
            NodePos found = nodePositions.Find(n => n.stateName == stateName);
            if (found != null)
            {
                position = found.position;
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        public void Set(string stateName, Vector2 position)
        {
            NodePos found = nodePositions.Find(n => n.stateName == stateName);
            if (found == null)
            {
                nodePositions.Add(new NodePos { stateName = stateName, position = position });
                return;
            }

            found.position = position;
        }
    }
}
