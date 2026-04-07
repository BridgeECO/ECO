using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergySwitch : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<EnergyLine> _connectedLines = new List<EnergyLine>();

    [Foldout("Project")]
    [SerializeField]
    private EInteractionType _interactionType = EInteractionType.Button;

    private bool _isPlayerInRange = false;
    private bool _isOn = false;

    private void Update()
    {
        if (!_isPlayerInRange)
        {
            return;
        }

        if (_interactionType == EInteractionType.Button && Input.GetKeyDown(KeyCode.F))
        {
            ToggleSwitch();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerInRange = true;

        if (_interactionType == EInteractionType.PressurePlate || _interactionType == EInteractionType.AutoPlay)
        {
            SetSwitchState(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerInRange = false;

        if (_interactionType == EInteractionType.PressurePlate || _interactionType == EInteractionType.AutoPlay)
        {
            SetSwitchState(false);
        }
    }

    private void ToggleSwitch()
    {
        SetSwitchState(!_isOn);
    }

    private void SetSwitchState(bool isOn)
    {
        if (_isOn == isOn)
        {
            return;
        }

        _isOn = isOn;

        foreach (EnergyLine line in _connectedLines)
        {
            if (line != null)
            {
                line.SetSwitchState(_isOn);
            }
        }
    }
}
