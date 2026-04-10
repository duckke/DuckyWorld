using DuckyWorld.Input;

namespace DuckyWorld.MiniGame
{
    /// <summary>
    /// 미니게임 상태
    /// None → Ready → Playing → End
    ///                Playing ↔ Paused
    /// </summary>
    public enum MiniGameState
    {
        None,
        Ready,
        Playing,
        Paused,
        End,
    }

    /// <summary>
    /// 모든 미니게임의 기본 추상 클래스
    /// 상태 전이는 MiniGameModule이 전담하며, SetState로만 변경된다.
    /// 각 미니게임은 이 클래스를 상속하여 게임 고유 로직을 구현한다.
    /// </summary>
    public abstract class MiniGameBase
    {
        /// <summary>이 미니게임의 종류</summary>
        public abstract GameType GameType { get; }

        /// <summary>현재 상태 (MiniGameModule이 SetState로 변경)</summary>
        public MiniGameState State { get; private set; } = MiniGameState.None;

        // --- 생명주기 콜백 ---

        /// <summary>Ready 진입 시 호출 — 게임 초기화, 오브젝트 배치</summary>
        public abstract void OnReady();

        /// <summary>Playing 진입 시 호출 — 게임 시작</summary>
        public abstract void OnPlay();

        /// <summary>Paused 진입 시 호출 — 로직 일시정지</summary>
        public abstract void OnPause();

        /// <summary>Paused → Playing 복귀 시 호출</summary>
        public abstract void OnResume();

        /// <summary>End 진입 시 호출 — 점수 확정, 정리</summary>
        public abstract void OnEnd();

        /// <summary>End → Ready 재시작 시 호출</summary>
        public abstract void OnRestart();

        // --- 프레임 루프 ---

        /// <summary>
        /// 게임 로직 업데이트.
        /// MiniGameModule.DoMainProc에서 Playing 상태일 때만 호출된다.
        /// </summary>
        public virtual void mainProc(float dt) { }

        // --- 추상 팩토리 메서드 ---

        /// <summary>이 게임이 사용하는 입력 핸들러 생성</summary>
        public abstract IGameInput CreateInput();

        /// <summary>이 게임에 맞는 MapManagerBase 구현체 생성</summary>
        public abstract MapManagerBase CreateMapManager();

        /// <summary>현재 점수 반환</summary>
        public abstract float GetScore();

        // --- MiniGameModule 전용 ---

        /// <summary>
        /// 상태 변경. MiniGameModule에서만 호출한다.
        /// </summary>
        internal void SetState(MiniGameState state)
        {
            State = state;
        }
    }
}
