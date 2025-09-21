/*
 * 프로그램의 전체 상태 
 * App보단 상위 (게임 보단 상위)
 * 
 * 
 */

using System;
using System.Diagnostics;
using UnityEngine;

namespace ECO
{
    public class Main : MonoBehaviour
    {
        private enum EState
        {
            NONE,
            INIT,       //초기화단계 -> 네트워크 확인, SDK Init (한번만 하면 되는 것들)
            READY,      //준비 단계 (재시작시 Ready부터 시작) -> Proto 로드하고, Config 로드, App 생성
            PLAY,       //실제 게임 시작 실제 시작
            RELOAD,     //재시작 디버그 용도 + 게임하다 보면 ~ 에러 나서 타이틀로 돌아갑니다
            ERROR,      //무언가 에러 났을 경우 (있으면 안되는) 
        }

        private Stopwatch _sw = new Stopwatch();
        private EState _curState = EState.NONE;

        private void OnDestroy()
        {

        }

        private void Awake()
        {
            EnterState(EState.INIT);
        }

        private void Update()
        {
            if (_curState <= EState.INIT)
                return;

            //App.Inst().Update();
            Debug_Update();
        }

        private void FixedUpdate()
        {

        }

        private void EnterState(EState state)
        {
            if (_curState == state)
                return;

            _sw.Stop();
            //LOG.I($"Enter State. BefState({_curState}), AftState({state}), EllapsedTime({_sw.Elapsed.ToSafeString()})");
            _sw.Restart();

            _curState = state;
            bool result = false;

            try
            {
                switch (state)
                {
                    case EState.INIT: result = EnterState_Init(); break;
                    case EState.READY: result = EnterState_Ready(); break;
                    case EState.PLAY: result = EnterState_Play(); break;
                    case EState.RELOAD: result = EnterState_Reload(); break;
                    case EState.ERROR: EnterState_Error(); break;
                    default:
                        //LOG.E($"No Handling State({state})");
                        break;
                }
            }
            catch (Exception exc)
            {
                //LOG.E($"Catch Exception. CurState({_curState}), Exc({exc})");
                result = false;
            }

            if (!result)
            {
                EnterState(EState.ERROR);
            }
        }

        private bool EnterState_Init()
        {
            //LOG.ClearLog();

            if (!App.Inst().Create())
            {
                //LOG.E("Create App Failed");
                return false;
            }

            EnterState(EState.READY);
            return true;
        }

        private bool EnterState_Ready()
        {
            //if (!App.Inst().CONFIG.TryLoad())
            //    return false;

            //if (!App.Inst().PROTO.TryLoad())
            //    return false;

            //App.Inst().DATA.LoadAllDataAsync();

            EnterState(EState.PLAY);
            return true;
        }

        //이미 씬이 로드 되어 있음
        //Main -> Game씬 로드
        private bool EnterState_Play()
        {
            //if (GAME.StartSceneName == TEXT.GAME_SIMULATOR_SCENE_NAME || GAME.StartSceneName == TEXT.SOUND_SIMULATOR_SCENE_NAME || GAME.StartSceneName == TEXT.YARN_SIMULATOR_SCENE_NAME)
            //{
            //    App.Inst().SCENE.LoadScene(GAME.StartSceneName);
            //    return true;
            //}

            //App.Inst().SCENE.LoadScene(TEXT.TITLE_SCENE_NAME);
            return true;
        }

        //재시작
        private bool EnterState_Reload()
        {
            //if (!App.Inst().IsCreateComplete)
            //{
            //    EnterState(EState.READY);
            //    return true;
            //}

            //App.Inst().SCENE.UnloadAllScene(EVENT_UnloadAllSceneComplete);
            return true;
        }

        private void EnterState_Error()
        {

        }

        public void RestartGame()
        {
            EnterState(EState.RELOAD);
        }

        [Conditional("ECO_DEBUG")]
        private void Debug_Update()
        {
            //여긴 디버그 기능이라 InputSystem을 사용하지 않는다.
            if (Input.GetKeyDown(KeyCode.F5))
            {
                EnterState(EState.RELOAD);
                return;
            }
        }
    }
}