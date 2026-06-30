using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NPCEventFlowRunner
{
    private readonly UI_NPCDialogue _uiNPCDialogue;
    private readonly NPCEventExecutor _executor;
    private CancellationTokenSource _interactionCts;
    private bool _isInteracting;

    public bool IsInteracting => _isInteracting;

    public NPCEventFlowRunner(UI_NPCDialogue uiNPCDialogue, NPCEventExecutor executor)
    {
        _uiNPCDialogue = uiNPCDialogue;
        _executor = executor;
    }

    public void StartInteraction(
        NPCEventSO defaultEvent,
        NPCEventSO conditionMetDefaultEvent,
        bool isConditionMet,
        NPCSpecialEventQueue specialEventQueue,
        PlayerInput playerInput)
    {
        _isInteracting = true;
        InputHandler.ChangeToUIInput();

        _interactionCts?.Dispose();
        _interactionCts = new CancellationTokenSource();

        InteractAsync(defaultEvent, conditionMetDefaultEvent, isConditionMet, specialEventQueue, playerInput, _interactionCts.Token).Forget();
    }

    public void CancelInteraction()
    {
        if (_isInteracting)
        {
            _interactionCts?.Cancel();
        }
        CloseDialogue();
    }

    public async UniTask<bool> RunEventFlowAsync(NPCEventSO eventData, PlayerInput playerInput, CancellationToken token)
    {
        if (eventData == null)
        {
            return false;
        }

        NPCEventSO nextEvent = eventData;
        bool isExecuted = false;

        while (nextEvent != null)
        {
            token.ThrowIfCancellationRequested();

            if (nextEvent.DialogueLines is not null && 0< nextEvent.DialogueLines.Length )
            {
                NPCEventSO chosenEvent = await ShowDialogueAndGetChoiceAsync(nextEvent, token);

                if (chosenEvent == null)
                {
                    return false;
                }

                if (chosenEvent != nextEvent)
                {
                    nextEvent = chosenEvent;
                    continue;
                }
            }

            _executor.Execute(nextEvent, playerInput);
            isExecuted = true;
            break;
        }

        return isExecuted;
    }

    private async UniTaskVoid InteractAsync(
        NPCEventSO defaultEvent,
        NPCEventSO conditionMetDefaultEvent,
        bool isConditionMet,
        NPCSpecialEventQueue specialEventQueue,
        PlayerInput playerInput,
        CancellationToken token)
    {
        try
        {
            if (specialEventQueue.HasActiveEvent)
            {
                await FireHighestPrioritySpecialEventAsync(specialEventQueue, playerInput, token);
            }
            else
            {
                await FireDefaultEventAsync(defaultEvent, conditionMetDefaultEvent, isConditionMet, playerInput, token);
            }
            await CloseDialogueAsync();
        }
        catch (OperationCanceledException)
        {
            CloseDialogue();
        }
        finally
        {
            InputHandler.ChangeToPlayerInput();
            _isInteracting = false;
        }
    }

    private async UniTask FireHighestPrioritySpecialEventAsync(
        NPCSpecialEventQueue specialEventQueue,
        PlayerInput playerInput,
        CancellationToken token)
    {
        NPCEventSO eventData = specialEventQueue.GetHighestPriority();
        if (eventData == null)
        {
            return;
        }

        bool isExecuted = await RunEventFlowAsync(eventData, playerInput, token);
        if (isExecuted)
        {
            specialEventQueue.MarkFired(eventData);
        }
    }

    private async UniTask FireDefaultEventAsync(
        NPCEventSO defaultEvent,
        NPCEventSO conditionMetDefaultEvent,
        bool isConditionMet,
        PlayerInput playerInput,
        CancellationToken token)
    {
        NPCEventSO targetEvent = isConditionMet && conditionMetDefaultEvent != null ? conditionMetDefaultEvent : defaultEvent;

        if (targetEvent == null)
        {
            return;
        }

        await RunEventFlowAsync(targetEvent, playerInput, token);
    }

    private async UniTask<NPCEventSO> ShowDialogueAndGetChoiceAsync(NPCEventSO eventData, CancellationToken token)
    {
        if (_uiNPCDialogue == null)
        {
            return eventData;
        }

        var tcs = new UniTaskCompletionSource<NPCEventSO>();

        using (token.Register(() => tcs.TrySetCanceled()))
        {
            _uiNPCDialogue.OnDialogueCompleted = null;
            _uiNPCDialogue.OnDialogueCompleted = () =>
            {
                if (eventData.HasChoices && eventData.Choices is not null && 0< eventData.Choices.Count )
                {
                    _uiNPCDialogue.ShowChoices(eventData.Choices, (selectedEvent) =>
                    {
                        tcs.TrySetResult(selectedEvent);
                    });
                }
                else
                {
                    tcs.TrySetResult(eventData);
                }
            };

            _uiNPCDialogue.Open(eventData.DialogueLines);

            try
            {
                return await tcs.Task;
            }
            catch (OperationCanceledException)
            {
                if (_uiNPCDialogue != null)
                {
                    _uiNPCDialogue.Close();
                }
                throw;
            }
            finally
            {
                if (_uiNPCDialogue != null)
                {
                    _uiNPCDialogue.OnDialogueCompleted = null;
                }
            }
        }
    }

    private void CloseDialogue()
    {
        if (_uiNPCDialogue != null)
        {
            _uiNPCDialogue.Close();
        }
    }

    private async UniTask CloseDialogueAsync()
    {
        if (_uiNPCDialogue != null)
        {
            await _uiNPCDialogue.CloseAsync();
        }
    }
}
