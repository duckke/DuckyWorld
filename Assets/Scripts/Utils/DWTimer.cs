using UnityEngine;

namespace DuckyWorld.Utils
{
    /// <summary>
    /// DuckyWorld Timer - GC 제로 지향
    /// 타이머, 카운트다운, 쿨다운 등 다양한 시간 관리 기능 제공
    /// </summary>
    public struct DWTimer
    {
        public float elapsed;
        public float duration;
        public bool isActive;

        public DWTimer(float duration)
        {
            this.duration = duration;
            this.elapsed = 0f;
            this.isActive = true;
        }

        /// <summary>
        /// 타이머 진행 (deltaTime 누적)
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!isActive) return;
            elapsed += deltaTime;
        }

        /// <summary>
        /// 타이머 종료 여부
        /// </summary>
        public bool IsFinished()
        {
            return elapsed >= duration;
        }

        /// <summary>
        /// 타이머 진행률 (0 ~ 1)
        /// </summary>
        public float Progress()
        {
            return Mathf.Clamp01(elapsed / duration);
        }

        /// <summary>
        /// 남은 시간
        /// </summary>
        public float Remaining()
        {
            return Mathf.Max(0f, duration - elapsed);
        }

        /// <summary>
        /// 타이머 초기화
        /// </summary>
        public void Reset()
        {
            elapsed = 0f;
            isActive = true;
        }

        /// <summary>
        /// 타이머 일시정지/재개
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
        }

        /// <summary>
        /// 타이머 즉시 완료 (테스트/디버그용)
        /// </summary>
        public void Complete()
        {
            elapsed = duration;
        }

        /// <summary>
        /// 카운트다운 진행 (역순)
        /// </summary>
        public void CountdownTick(float deltaTime)
        {
            if (!isActive) return;
            elapsed += deltaTime;
        }

        /// <summary>
        /// 카운트다운 남은 시간 표시
        /// </summary>
        public float CountdownRemaining()
        {
            return Mathf.Max(0f, duration - elapsed);
        }

        /// <summary>
        /// 쿨다운 체크 및 시작 (쿨다운 시간이 남아있으면 true)
        /// </summary>
        public bool IsOnCooldown()
        {
            return elapsed < duration;
        }
    }
}
