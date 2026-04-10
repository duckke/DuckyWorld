using UnityEngine;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 몬스터 기본 클래스
    /// ActiveObject를 상속하여 AI 패턴, 공격 패턴 등 제공
    /// </summary>
    public class MonsterBase : ActiveObject
    {
        // AI 패턴 (정수로 상태 표현: 0=Idle, 1=Chase, 2=Attack 등)
        public int aiPattern { get; set; } = 0;

        // 공격 패턴
        public int attackPattern { get; set; } = 0;

        // 감지 범위
        public float detectionRange { get; set; } = 10f;

        // 공격 범위
        public float attackRange { get; set; } = 2f;

        // 추적 대상 (CharacterBase 등)
        protected ObjectBase m_targetObject = null;

        public MonsterBase()
        {
            poolKey = ObjectType.Monster;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            aiPattern = 0;  // Idle
            attackPattern = 0;
            m_targetObject = null;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            aiPattern = 0;
            attackPattern = 0;
            m_targetObject = null;
        }

        /// <summary>
        /// mainProc - AI 로직 실행
        /// </summary>
        public override void mainProc(float dt)
        {
            base.mainProc(dt);

            // AI 패턴 실행
            switch (aiPattern)
            {
                case 0:  // Idle
                    UpdateIdleState(dt);
                    break;
                case 1:  // Chase
                    UpdateChaseState(dt);
                    break;
                case 2:  // Attack
                    UpdateAttackState(dt);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Idle 상태 업데이트
        /// 대상 감지
        /// </summary>
        protected virtual void UpdateIdleState(float dt)
        {
            // 감지 범위 내 대상 찾기
            m_targetObject = FindTargetInRange(detectionRange);

            if (m_targetObject != null)
            {
                aiPattern = 1;  // Chase 상태로 전환
            }
        }

        /// <summary>
        /// Chase 상태 업데이트
        /// 대상 추적
        /// </summary>
        protected virtual void UpdateChaseState(float dt)
        {
            if (m_targetObject == null)
            {
                aiPattern = 0;  // Idle로 돌아감
                return;
            }

            float distance = Vector3.Distance(position, m_targetObject.position);

            if (distance <= attackRange)
            {
                aiPattern = 2;  // Attack 상태로 전환
                return;
            }

            // 대상 방향으로 이동
            Vector3 moveDir = (m_targetObject.position - position).normalized;
            position += moveDir * moveSpeed * dt;

            // 방향 업데이트
            facingDirection = moveDir.x > 0 ? 1 : -1;
        }

        /// <summary>
        /// Attack 상태 업데이트
        /// 대상 공격
        /// </summary>
        protected virtual void UpdateAttackState(float dt)
        {
            if (m_targetObject == null)
            {
                aiPattern = 0;  // Idle로 돌아감
                return;
            }

            float distance = Vector3.Distance(position, m_targetObject.position);

            if (distance > attackRange + 1f)
            {
                aiPattern = 1;  // Chase로 돌아감
                return;
            }

            // 공격 패턴 실행
            switch (attackPattern)
            {
                case 0:  // 기본 공격
                    ExecuteNormalAttack();
                    break;
                case 1:  // 특수 공격 1
                    ExecuteSpecialAttack1();
                    break;
                case 2:  // 특수 공격 2
                    ExecuteSpecialAttack2();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 감지 범위 내에서 대상 찾기
        /// 상속 클래스에서 오버라이드
        /// </summary>
        protected virtual ObjectBase FindTargetInRange(float range)
        {
            // 오버라이드 필요
            // ModuleBase에 등록된 Character 목록에서 조회
            return null;
        }

        /// <summary>
        /// 기본 공격
        /// </summary>
        protected virtual void ExecuteNormalAttack()
        {
            // 오버라이드 필요
            // 공격 애니메이션, 피해 처리 등
        }

        /// <summary>
        /// 특수 공격 1
        /// </summary>
        protected virtual void ExecuteSpecialAttack1()
        {
            // 오버라이드 필요
        }

        /// <summary>
        /// 특수 공격 2
        /// </summary>
        protected virtual void ExecuteSpecialAttack2()
        {
            // 오버라이드 필요
        }

        /// <summary>
        /// 입력 처리 (몬스터는 무시)
        /// </summary>
        public override void ApplyInput(InputData inputData)
        {
            // 몬스터는 입력을 받지 않음
        }

        /// <summary>
        /// 현재 추적 대상 반환
        /// </summary>
        public ObjectBase GetTarget()
        {
            return m_targetObject;
        }

        /// <summary>
        /// 강제 대상 설정 (테스트용)
        /// </summary>
        public void SetTarget(ObjectBase target)
        {
            m_targetObject = target;
            if (target != null)
            {
                aiPattern = 1;  // Chase 시작
            }
        }
    }
}
