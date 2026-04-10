using UnityEngine;

namespace DuckyWorld.MiniGame
{
    /// <summary>
    /// 미니게임 종류 식별자
    /// MiniGameFactory에서 올바른 구현체 생성에 사용
    /// </summary>
    public enum GameType
    {
        ThumpThumpSlope,    // 두근두근 슬로프
        FlutterFlutter,     // 퍼덕퍼덕
        WaddleSprint,       // 뒤뚱 달리기
        HoppyForest,        // 깡충 숲
        SlideRun,           // 슬라이드 런
        NarrowPath,         // 좁은 길
        DodgeVillain,       // 악당 피하기
    }

    /// <summary>
    /// 미니게임 생성 팩토리
    /// GameType에 맞는 MiniGameBase 구현체를 반환한다.
    /// 구현체가 없으면 null 반환 (호출부에서 처리)
    /// </summary>
    public static class MiniGameFactory
    {
        /// <summary>
        /// GameType에 대응하는 MiniGameBase 구현체 생성
        /// 각 미니게임별 서브클래스가 없으면 null 반환
        /// </summary>
        public static MiniGameBase Create(GameType gameType)
        {
            switch (gameType)
            {
                // 구현체가 추가될 때마다 case 추가
                // case GameType.ThumpThumpSlope: return new ThumpThumpSlope();
                // case GameType.FlutterFlutter:  return new FlutterFlutter();

                default:
                    Debug.LogWarning($"[MiniGameFactory] 구현체 없음: {gameType}. null 반환.");
                    return null;
            }
        }
    }
}
