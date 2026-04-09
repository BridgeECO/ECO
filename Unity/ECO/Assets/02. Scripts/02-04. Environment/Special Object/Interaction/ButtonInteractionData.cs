using UnityEngine;

public class ButtonInteractionData : MonoBehaviour
{
    [SerializeField]
    private float _deactivateDelayTime = 3f;

    public float DeactivateDelayTime => _deactivateDelayTime;
}
