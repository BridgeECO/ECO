#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 조립을 잘못했을 때 조용히 망가지는 대신 바로 알려 준다.
/// 콘솔 줄을 클릭하면 문제의 Director가 씬에서 핑된다.
/// </summary>
public static class CutsceneFlowValidator
{
    private static readonly HashSet<CutsceneStepBase> _visitStack = new HashSet<CutsceneStepBase>();

    // 목록 전체를 훑어야 알 수 있는 것들. 재귀에 ref를 줄줄이 넘기지 않으려고 여기 둔다.
    private static bool _hasControlRelease;
    private static bool _hasCameraMove;
    private static bool _hasCameraReturn;

    public static void Validate(CutsceneDirector director)
    {
        if (director == null)
        {
            return;
        }

        WarnMissingRoom(director);

        _visitStack.Clear();
        _hasControlRelease = false;
        _hasCameraMove = false;
        _hasCameraReturn = false;

        ValidateList(director, director.Steps, 1, false);
        WarnMissingClosers(director);
    }

    /// <summary>
    /// 빼앗은 것을 돌려주는 스텝이 없으면 컷씬이 끝난 뒤에도 화면이 어색하게 남는다.
    /// 세션이 소유권 자체는 반드시 반납하므로 에러가 아니라 경고다.
    /// </summary>
    private static void WarnMissingClosers(CutsceneDirector director)
    {
        if (director.IsFreezePlayer && !_hasControlRelease)
        {
            Debug.LogWarning(
                $"[CutsceneDirector] '{director.name}': 플레이어를 세우는데 조작을 돌려주는 스텝이 없습니다. " +
                "마지막에 '플레이어 조작' 스텝을 두고 체크를 해제하세요.", director);
        }

        if (_hasCameraMove && !_hasCameraReturn)
        {
            Debug.LogWarning(
                $"[CutsceneDirector] '{director.name}': 카메라를 옮기는데 되돌리는 스텝이 없습니다. " +
                "마지막에 '카메라 이동' 스텝을 두고 지점을 비우면 플레이어 쪽으로 돌아옵니다.", director);
        }
    }

    /// <summary>
    /// 상위에 Room이 없으면 Room.ResetRoom의 IResettable 수집에 잡히지 않는다.
    /// 리스폰해도 컷씬 진행 상태가 되돌아가지 않고 연출 오브젝트도 그대로 남는다.
    /// </summary>
    private static void WarnMissingRoom(CutsceneDirector director)
    {
        if (director.GetComponentInParent<Room>(true) != null)
        {
            return;
        }

        Debug.LogWarning(
            $"[CutsceneDirector] '{director.name}'이(가) Room 하위에 있지 않습니다. " +
            "리스폰해도 컷씬 진행 상태와 연출 오브젝트가 되돌아가지 않습니다.", director);
    }

    private static void ValidateList(CutsceneDirector director, IReadOnlyList<CutsceneStepBase> steps,
        int depth, bool isInsideParallel)
    {
        if (steps == null)
        {
            return;
        }

        if (CutsceneGroupStepBase.MAX_NEST_DEPTH < depth)
        {
            Debug.LogError(
                $"[CutsceneDirector] '{director.name}'의 그룹 중첩이 상한({CutsceneGroupStepBase.MAX_NEST_DEPTH})을 " +
                "넘었습니다. 재귀가 스택을 넘기면 에디터가 그대로 종료됩니다.", director);
            return;
        }

        for (int i = 0; i < steps.Count; i++)
        {
            CutsceneStepBase step = steps[i];
            if (ReferenceEquals(step, null))
            {
                continue;
            }

            ValidateStep(director, step, i, depth, isInsideParallel);
        }
    }

    private static void ValidateStep(CutsceneDirector director, CutsceneStepBase step, int index,
        int depth, bool isInsideParallel)
    {
        // 그룹 토큰은 스킵 토큰에 묶여 있어, 끊으면 망가지는 스텝의 "끝까지 실행" 계약이 깨진다.
        if (isInsideParallel && !step.IsSkippable)
        {
            Debug.LogError(
                $"[CutsceneDirector] '{director.name}' {index + 1}번: 끊을 수 없는 스텝은 " +
                "동시 실행 그룹 안에 넣을 수 없습니다.", director);
        }

        if (step is CutscenePlayerControlStep controlStep && !controlStep.IsControlBlocked)
        {
            _hasControlRelease = true;
        }

        if (step is CutsceneCameraMoveStep cameraStep)
        {
            _hasCameraMove = true;
            _hasCameraReturn = _hasCameraReturn || cameraStep.ViewPoint == null;
        }

        // 반복 재생은 스스로 끝나지 않는다. 순차 자리에 두면 컷씬이 영영 다음으로 못 넘어간다.
        if (step is CutsceneActorSpriteStep spriteStep && spriteStep.IsLoop && !isInsideParallel)
        {
            Debug.LogError(
                $"[CutsceneDirector] '{director.name}' {index + 1}번: 반복 재생 스프라이트는 " +
                "동시 실행 그룹 안에서만 쓸 수 있습니다. 순차 자리에서는 끝나지 않습니다.", director);
        }

        if (!(step is CutsceneGroupStepBase group))
        {
            return;
        }

        // 유니티는 순환 참조를 파일에 그대로 저장하고 다시 읽는다. 한 번 생기면 열 때마다 재현된다.
        if (!_visitStack.Add(step))
        {
            Debug.LogError(
                $"[CutsceneDirector] '{director.name}' {index + 1}번: 그룹이 자기 자신을 포함하고 있습니다.", director);
            return;
        }

        ValidateList(director, group.Steps, depth + 1, group is CutsceneParallelGroupStep);
        _visitStack.Remove(step);
    }
}
#endif
