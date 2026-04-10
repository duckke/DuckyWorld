using UnityEngine;
using DuckyWorld.Core;

namespace DuckyWorld.MiniGame
{
    /// <summary>
    /// 미니게임 진행 전담 모듈
    /// 상태 전이(None→Ready→Playing↔Paused→End)를 관리한다.
    ///
    /// 흐름:
    ///   OnEnter() → Factory로 게임 생성 → Ready
    ///   StartGame()  → Playing
    ///   PauseGame()  → Paused
    ///   ResumeGame() → Playing
    ///   EndGame()    → End
    ///   RestartGame()→ Ready
    ///   OnExit()     → 정리
    /// </summary>
    public class MiniGameModule : ModuleBase
    {
        public GameType CurrentGameType { get; private set; }
        public MiniGameBase CurrentGame { get; private set; }
        public MapManagerBase MapManager { get; private set; }

        // --- 진입/이탈 ---

        /// <summary>
        /// 씬 진입 시 호출.
        /// Factory로 게임을 생성하고 Ready 상태로 전이한다.
        /// </summary>
        public override void OnEnter()
        {
            CurrentGame = MiniGameFactory.Create(CurrentGameType);
            if (CurrentGame == null)
            {
                Debug.LogError($"[MiniGameModule] 게임 생성 실패: {CurrentGameType}");
                return;
            }

            MapManager = CurrentGame.CreateMapManager();

            // None → Ready
            CurrentGame.SetState(MiniGameState.Ready);
            MapManager?.OnReady();
            CurrentGame.OnReady();

            Debug.Log($"[MiniGameModule] OnEnter → Ready ({CurrentGameType})");
        }

        /// <summary>
        /// 씬 이탈 시 호출 — 게임 및 맵 정리
        /// </summary>
        public override void OnExit()
        {
            if (CurrentGame != null && CurrentGame.State != MiniGameState.End)
            {
                MapManager?.OnEnd();
                CurrentGame.SetState(MiniGameState.End);
                CurrentGame.OnEnd();
            }

            CurrentGame = null;
            MapManager = null;

            Debug.Log($"[MiniGameModule] OnExit 완료 ({CurrentGameType})");
        }

        // --- 상태 전이 메서드 (UI가 호출) ---

        /// <summary>Ready → Playing. UI 카운트다운 완료 후 UI가 호출.</summary>
        public void StartGame()
        {
            if (CurrentGame == null) return;
            CurrentGame.SetState(MiniGameState.Playing);
            CurrentGame.OnPlay();
            Debug.Log("[MiniGameModule] StartGame → Playing");
        }

        /// <summary>Playing → Paused. UI 일시정지 버튼.</summary>
        public void PauseGame()
        {
            if (CurrentGame == null) return;
            CurrentGame.SetState(MiniGameState.Paused);
            CurrentGame.OnPause();
            Debug.Log("[MiniGameModule] PauseGame → Paused");
        }

        /// <summary>Paused → Playing. UI 재개 버튼.</summary>
        public void ResumeGame()
        {
            if (CurrentGame == null) return;
            CurrentGame.SetState(MiniGameState.Playing);
            CurrentGame.OnResume();
            Debug.Log("[MiniGameModule] ResumeGame → Playing");
        }

        /// <summary>Playing → End. 게임 내부 조건 충족 시 CurrentGame이 호출 요청.</summary>
        public void EndGame()
        {
            if (CurrentGame == null) return;
            MapManager?.OnEnd();
            CurrentGame.SetState(MiniGameState.End);
            CurrentGame.OnEnd();
            Debug.Log("[MiniGameModule] EndGame → End");
        }

        /// <summary>End → Ready. UI 재시작 버튼.</summary>
        public void RestartGame()
        {
            if (CurrentGame == null) return;
            MapManager?.OnReady();
            CurrentGame.SetState(MiniGameState.Ready);
            CurrentGame.OnRestart();
            Debug.Log("[MiniGameModule] RestartGame → Ready");
        }

        // --- 프레임 루프 ---

        public override void DoMainProc()
        {
            base.DoMainProc();

            // Playing 상태일 때만 게임/맵 로직 실행
            if (CurrentGame != null && CurrentGame.State == MiniGameState.Playing)
            {
                float dt = Time.deltaTime;
                MapManager?.mainProc(dt);
                CurrentGame.mainProc(dt);
            }
        }

        // --- 초기화 ---

        /// <summary>
        /// 어떤 미니게임을 실행할지 설정한다.
        /// OnEnter() 호출 전에 반드시 설정되어야 한다.
        /// </summary>
        public void SetGameType(GameType gameType)
        {
            CurrentGameType = gameType;
        }
    }
}
