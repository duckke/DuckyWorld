using UnityEngine;

namespace DuckyWorld.Object
{
    /// <summary>
    /// ObjectBase에 내장된 애니메이션 프레임 관리자
    /// GC 제로 설계: 단순 정수 상태만 관리, 데이터는 외부에서 전달
    /// </summary>
    public class LogicAnimator
    {
        // 현재 애니메이션 상태
        private int m_currentAnimId = -1;
        private int m_currentFrame = 0;
        private float m_frameTimer = 0f;
        private float m_frameDuration = 0f;  // 1/fps
        private int m_totalFrames = 0;
        private bool m_isLooping = false;

        public int CurrentAnimId => m_currentAnimId;
        public int CurrentFrame => m_currentFrame;
        public bool IsAnimationPlaying => m_currentAnimId >= 0;

        public LogicAnimator()
        {
            Reset();
        }

        /// <summary>
        /// 애니메이션 시작 또는 변경
        /// </summary>
        /// <param name="animId">애니메이션 ID</param>
        /// <param name="fps">프레임 레이트 (프레임당 시간)</param>
        /// <param name="totalFrames">전체 프레임 수</param>
        /// <param name="isLooping">반복 여부</param>
        public void Play(int animId, float fps, int totalFrames, bool isLooping = false)
        {
            m_currentAnimId = animId;
            m_currentFrame = 0;
            m_frameTimer = 0f;
            m_frameDuration = 1f / fps;
            m_totalFrames = totalFrames;
            m_isLooping = isLooping;
        }

        /// <summary>
        /// 프레임 진행 (매 프레임 호출, ObjectBase.preProc에서)
        /// </summary>
        /// <returns>프레임이 변경되었으면 true</returns>
        public bool UpdateFrame(float dt)
        {
            if (m_currentAnimId < 0) return false;

            m_frameTimer += dt;

            if (m_frameTimer >= m_frameDuration)
            {
                m_frameTimer -= m_frameDuration;
                m_currentFrame++;

                if (m_currentFrame >= m_totalFrames)
                {
                    if (m_isLooping)
                    {
                        m_currentFrame = 0;
                    }
                    else
                    {
                        m_currentFrame = m_totalFrames - 1;
                        // 애니메이션 종료 (필요시 OnAnimationEnd 호출 가능)
                    }
                }

                return true;  // 프레임 변경됨 → collider 인덱스 갱신 필요
            }

            return false;
        }

        /// <summary>
        /// 애니메이션 초기화
        /// </summary>
        public void Stop()
        {
            m_currentAnimId = -1;
            m_currentFrame = 0;
            m_frameTimer = 0f;
        }

        /// <summary>
        /// 현재 프레임 강제 설정 (디버그/초기화용)
        /// </summary>
        public void SetFrame(int frame)
        {
            m_currentFrame = Mathf.Clamp(frame, 0, Mathf.Max(0, m_totalFrames - 1));
        }

        /// <summary>
        /// 현재 애니메이션의 진행률 (0 ~ 1)
        /// </summary>
        public float GetProgress()
        {
            if (m_totalFrames <= 0) return 0f;
            return (float)m_currentFrame / (m_totalFrames - 1);
        }

        /// <summary>
        /// 내부 상태 초기화
        /// </summary>
        private void Reset()
        {
            m_currentAnimId = -1;
            m_currentFrame = 0;
            m_frameTimer = 0f;
            m_frameDuration = 0f;
            m_totalFrames = 0;
            m_isLooping = false;
        }
    }
}
