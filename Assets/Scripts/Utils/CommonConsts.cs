namespace DuckyWorld.Utils
{
    /// <summary>
    /// 프로젝트 전역 상수 정의
    /// </summary>
    public static class CommonConsts
    {
        // 프레임 타이밍
        public const float FIXED_DELTA_TIME = 0.02f;  // 50Hz 로직
        public const int TARGET_FPS = 60;

        // 풀 기본값
        public const int DEFAULT_POOL_SIZE = 32;
        public const int DEFAULT_POOL_PREWARM = 16;

        // 레이어 & 태그
        public const string TAG_PLAYER = "Player";
        public const string TAG_OBSTACLE = "Obstacle";
        public const string TAG_COLLECTIBLE = "Collectible";

        // UI 상수
        public const float UI_FADE_DURATION = 0.3f;

        // 네트워크 (향후 사용)
        public const float NETWORK_SYNC_INTERVAL = 0.1f;  // 10Hz
    }
}
