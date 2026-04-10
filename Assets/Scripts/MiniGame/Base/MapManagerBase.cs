using UnityEngine;

namespace DuckyWorld.MiniGame
{
    /// <summary>
    /// 맵 생성/관리 추상 기반 클래스
    /// 모든 미니게임 공통 맵 정보를 보유하며, 게임별 세부 로직은 구현체 책임.
    ///
    /// MapData(Phase 9)가 구현되면 Init(MapData) 오버라이드에서 필드를 채운다.
    /// </summary>
    public abstract class MapManagerBase
    {
        // === 공통 정보 (모든 미니게임 공유) ===
        public float mapWidth;      // 맵 가로 크기
        public float mapHeight;     // 맵 세로 크기
        public float scrollSpeed;   // 맵 스크롤 속도
        public Rect spawnArea;      // 오브젝트 스폰 가능 영역
        public Rect despawnArea;    // 오브젝트 회수 영역 (화면 밖)

        // --- 생명주기 ---

        /// <summary>Ready 시 맵 초기 세팅 — 오브젝트 초기 배치 등</summary>
        public abstract void OnReady();

        /// <summary>Playing 중 매 프레임 — 스크롤, 스폰 타이밍, 난이도 조절</summary>
        public abstract void mainProc(float dt);

        /// <summary>End 시 맵 정리 — 오브젝트 회수 등</summary>
        public abstract void OnEnd();
    }
}
