using System;
using UnityEngine;

[Serializable]
public struct NPCChoiceOption
{
    public string ChoiceText;
    public NPCEventSO NextEvent;
}
