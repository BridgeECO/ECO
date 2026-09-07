using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NPCEventExecutor
{
    private readonly IReadOnlyList<CutsceneDirector> _cutscenes;

    public NPCEventExecutor(IReadOnlyList<CutsceneDirector> cutscenes)
    {
        _cutscenes = cutscenes;
    }

    /// <summary>
    /// 실행에 성공하면 true. 실패를 false로 올려야 호출부가 이 이벤트를 큐에 남긴다.
    /// 조용히 true를 돌려주면 발동한 적 없는 이벤트가 영영 사라진다.
    /// </summary>
    public async UniTask<bool> ExecuteAsync(NPCEventSO eventData, PlayerInput playerInput,
        CancellationToken cancellationToken)
    {
        switch (eventData.EventType)
        {
            case ENPCEventType.UnlockAbility:
                return ExecuteUnlockAbility(eventData.AbilityToUnlock, playerInput);

            case ENPCEventType.ActivateGimmick:
                ExecuteActivateGimmick(eventData);
                return true;

            case ENPCEventType.PlayCutscene:
                return await ExecutePlayCutsceneAsync(eventData, playerInput, cancellationToken);
        }

        return true;
    }

    private async UniTask<bool> ExecutePlayCutsceneAsync(NPCEventSO eventData, PlayerInput playerInput,
        CancellationToken cancellationToken)
    {
        CutsceneDirector director = FindCutscene(eventData.CutsceneId);
        if (director == null)
        {
            Debug.LogError(
                $"[NPCEventExecutor] '{eventData.name}'이(가) 지목한 컷씬 '{eventData.CutsceneId}'을(를) " +
                "NPC의 컷씬 목록에서 찾지 못했습니다.");
            return false;
        }

        // 컷씬이 플레이어를 세우려면 참조가 필요한데, NPC 경로에서는 콜라이더 대신 이쪽으로 들어온다.
        if (playerInput != null)
        {
            director.BindPlayer(playerInput.GetComponent<PlayerStateMachine>());
        }

        await director.PlayAsync(cancellationToken);
        return true;
    }

    private CutsceneDirector FindCutscene(string cutsceneId)
    {
        if (_cutscenes == null || string.IsNullOrEmpty(cutsceneId))
        {
            return null;
        }

        for (int i = 0; i < _cutscenes.Count; i++)
        {
            CutsceneDirector director = _cutscenes[i];
            if (director != null && director.CutsceneId == cutsceneId)
            {
                return director;
            }
        }

        return null;
    }

    private bool ExecuteUnlockAbility(EPlayerUnlockableAbility abilityType, PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            return false;
        }

        // 플레이어 내부 잠금 상태를 직접 대입하지 않고 의도 메서드를 경유한다.
        playerInput.UnlockAbility(abilityType);
        return true;
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
