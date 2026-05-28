using UnityEngine;

public class NPCEventExecutor
{
    public void Execute(NPCEventSO eventData, PlayerInput playerInput)
    {
        switch (eventData.EventType)
        {
            case ENPCEventType.UnlockAbility:
                ExecuteUnlockAbility(eventData.AbilityToUnlock, playerInput);
                break;
            case ENPCEventType.ActivateGimmick:
                ExecuteActivateGimmick(eventData);
                break;
            case ENPCEventType.PlayCutscene:
                ExecutePlayCutscene(eventData);
                break;
        }
    }

    private void ExecutePlayCutscene(NPCEventSO eventData)
    {
        // TODO: Implement cutscene execution logic
    }

    private void ExecuteUnlockAbility(EPlayerUnlockableAbility abilityType, PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            return;
        }

        switch (abilityType)
        {
            case EPlayerUnlockableAbility.Dash:
                playerInput.IsDashLocked = false;
                break;
            case EPlayerUnlockableAbility.WallSlide:
                playerInput.IsWallSlideLocked = false;
                break;
        }
    }

    private void ExecuteActivateGimmick(NPCEventSO eventData)
    {
        for (int i = 0; i < eventData.TargetTerrains.Count; i++)
        {
            if (eventData.TargetTerrains[i] != null)
            {
                eventData.TargetTerrains[i].SetEnergyActive(eventData.IsGimmickActive);
            }
        }
    }
}
