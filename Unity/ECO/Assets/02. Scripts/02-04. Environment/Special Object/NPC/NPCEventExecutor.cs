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
        Debug.LogWarning($"[NPCEventExecutor] 컷신 실행은 아직 구현되지 않았습니다. ({eventData.name})");
    }

    private void ExecuteUnlockAbility(EPlayerUnlockableAbility abilityType, PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            return;
        }

        // 플레이어 내부 잠금 상태를 직접 대입하지 않고 의도 메서드를 경유한다.
        playerInput.UnlockAbility(abilityType);
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
