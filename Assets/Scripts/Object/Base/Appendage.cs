using UnityEngine;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 버프/디버프 시스템
    /// ActiveObject에 동적으로 붙어 지속시간 동안 효과 제공
    /// 각 appendage는 독립적인 라이프사이클을 가짐
    /// </summary>
    public class Appendage
    {
        // 그룹 ID (같은 ID의 여러 appendage 가능)
        public int appendageId { get; set; }

        // 타입 (SpeedUp, Invincible, Stun 등)
        public AppendageType type { get; set; }

        // 지속시간
        public float duration { get; set; }
        public float elapsed { get; set; }

        // 효과값 (타입에 따라 해석)
        public float value { get; set; }

        public Appendage()
        {
            appendageId = 0;
            type = AppendageType.SpeedUp;
            duration = 0f;
            elapsed = 0f;
            value = 0f;
        }

        public Appendage(int id, AppendageType appType, float dur, float val)
        {
            appendageId = id;
            type = appType;
            duration = dur;
            elapsed = 0f;
            value = val;
        }

        /// <summary>
        /// preProc - 프레임 시작 처리
        /// </summary>
        public virtual void preProc(float dt)
        {
            // 오버라이드 가능
        }

        /// <summary>
        /// mainProc - 지속시간 진행
        /// </summary>
        /// <returns>계속 활성화되어야 하면 true, 만료되면 false</returns>
        public virtual bool mainProc(float dt)
        {
            elapsed += dt;
            return elapsed < duration;
        }

        /// <summary>
        /// postProc - 프레임 종료 처리
        /// </summary>
        public virtual void postProc(float dt)
        {
            // 오버라이드 가능
        }

        /// <summary>
        /// 대상 오브젝트에 부착될 때 호출
        /// </summary>
        public virtual void OnAttach(ActiveObject target)
        {
            // 오버라이드 가능
            // 예: 속도 증가 이펙트 적용, 버프 아이콘 표시 등
        }

        /// <summary>
        /// 대상 오브젝트에서 제거될 때 호출
        /// </summary>
        public virtual void OnDetach(ActiveObject target)
        {
            // 오버라이드 가능
            // 예: 원래 속도로 복원, 버프 아이콘 제거 등
        }

        /// <summary>
        /// 지속시간 진행률 (0 ~ 1)
        /// </summary>
        public float GetProgress()
        {
            if (duration <= 0) return 0f;
            return Mathf.Clamp01(elapsed / duration);
        }

        /// <summary>
        /// 남은 시간
        /// </summary>
        public float GetRemainingDuration()
        {
            return Mathf.Max(0f, duration - elapsed);
        }

        /// <summary>
        /// 만료 여부
        /// </summary>
        public bool IsExpired()
        {
            return elapsed >= duration;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Reset()
        {
            elapsed = 0f;
        }
    }
}
