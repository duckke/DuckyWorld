using UnityEngine;
using DuckyWorld.Utils;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 모든 로직 오브젝트의 기본 클래스 (하이어라키 없음, 순수 C# 클래스)
    /// GameManager의 preProc→mainProc→postProc 체인에 참여
    /// </summary>
    public class ObjectBase
    {
        // 정적 ID 카운터
        private static int s_nextObjectId = 0;

        // 기본 정보
        public int objectId { get; private set; }
        public string objectName { get; set; }

        // 로직 좌표 (View와 동기화됨)
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }

        // 타이머 (쿨다운, 지속 효과 등)
        public DWTimer objectTimer { get; private set; }

        // View 연동
        public int viewId { get; set; } = -1;  // -1 = View 없음

        // 풀 관리
        public ObjectType poolKey { get; set; }

        // 애니메이션 관리자 (내장)
        public LogicAnimator logicAnimator { get; private set; }

        public ObjectBase()
        {
            objectId = s_nextObjectId++;
            objectName = this.GetType().Name + "_" + objectId;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            objectTimer = new DWTimer(0f);
            logicAnimator = new LogicAnimator();
        }

        /// <summary>
        /// 스폰 시 호출 (풀에서 꺼낼 때)
        /// 상속 클래스에서 오버라이드하여 초기화 수행
        /// </summary>
        public virtual void OnSpawn()
        {
            objectTimer.Reset();
        }

        /// <summary>
        /// 회수 시 호출 (풀에 반환할 때)
        /// 상속 클래스에서 오버라이드하여 정리 수행
        /// </summary>
        public virtual void OnDespawn()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            viewId = -1;
        }

        /// <summary>
        /// preProc - 입력, 상태 변경 전 준비 (각 프레임 시작)
        /// 애니메이션 프레임 진행, 타이머 갱신 등
        /// 프레임 변경 시 충돌체 인덱스 갱신 (CollisionObject에서)
        /// </summary>
        public virtual void preProc(float dt)
        {
            objectTimer.Tick(dt);

            // 애니메이션 프레임 업데이트 및 변경 감지
            bool frameChanged = logicAnimator.UpdateFrame(dt);
            if (frameChanged && this is CollisionObject co)
            {
                // 프레임 변경 시 활성 충돌체 범위 업데이트
                co.UpdateActiveColliderRange(logicAnimator.CurrentAnimId, logicAnimator.CurrentFrame);
            }
        }

        /// <summary>
        /// mainProc - 위치 이동, 로직 업데이트
        /// </summary>
        public virtual void mainProc(float dt)
        {
            // 오버라이드 필요
        }

        /// <summary>
        /// postProc - 충돌 처리, 뷰 동기화 (프레임 종료)
        /// </summary>
        public virtual void postProc(float dt)
        {
            // 오버라이드 필요
        }

        /// <summary>
        /// 정적 ID 카운터 초기화 (테스트용)
        /// </summary>
        public static void ResetIdCounter()
        {
            s_nextObjectId = 0;
        }
    }

    /// <summary>
    /// 오브젝트 타입 (풀 관리용)
    /// </summary>
    public enum ObjectType
    {
        Unknown = 0,
        Character = 1,
        Monster = 2,
        Obstacle = 3,
        Projectile = 4,
        Item = 5,
        Effect = 6,
    }
}
