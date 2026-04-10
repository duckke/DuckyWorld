using DuckyWorld.Object;

namespace DuckyWorld.Factory
{
    /// <summary>
    /// Appendage(버프/디버프) 생성 팩토리
    /// AppendageType별로 올바른 Appendage 인스턴스 생성
    /// GC 최소화: new는 PoolManager가 풀에서 꺼낼 때만 수행
    /// </summary>
    public static class AppendageFactory
    {
        /// <summary>
        /// AppendageType에 따라 해당 타입의 Appendage 인스턴스 생성
        /// 풀에서 객체가 없을 때만 호출됨 (PoolManager.GetAppendage에서)
        /// </summary>
        public static Appendage Create(AppendageType type)
        {
            // 새 인스턴스 생성 (PoolManager가 풀 부족 시만 호출)
            var appendage = new Appendage();

            // 타입별 초기 설정
            InitializeByType(appendage, type);

            return appendage;
        }

        /// <summary>
        /// 타입별 Appendage 초기화 로직
        /// 풀에서 꺼낼 때마다 Reset에서 이 값들을 재설정하지 않도록 주의
        /// </summary>
        private static void InitializeByType(Appendage appendage, AppendageType type)
        {
            appendage.type = type;

            switch (type)
            {
                case AppendageType.SpeedUp:
                    // 속도 증가 버프
                    // value: 속도 배율 (예: 1.5f = 150%)
                    appendage.duration = 5f;
                    appendage.value = 1.5f;
                    break;

                case AppendageType.Invincible:
                    // 무적 상태
                    // value: 무시 (불린처럼 존재 여부로만 사용 가능)
                    appendage.duration = 3f;
                    appendage.value = 1f;
                    break;

                case AppendageType.Stun:
                    // 기절 상태
                    // value: 움직임 제한 정도 (0 = 완전 정지, 1 = 일부 감소)
                    appendage.duration = 1f;
                    appendage.value = 1f;  // 완전 정지
                    break;

                case AppendageType.SlowDown:
                    // 속도 감소 디버프
                    // value: 속도 배율 (예: 0.5f = 50% 속도)
                    appendage.duration = 4f;
                    appendage.value = 0.5f;
                    break;

                default:
                    UnityEngine.Debug.LogWarning($"[AppendageFactory] 알 수 없는 AppendageType: {type}");
                    break;
            }
        }
    }
}
