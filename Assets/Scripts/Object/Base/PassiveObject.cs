using UnityEngine;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 조작 불가능, 자체 이동만 가능한 오브젝트
    /// 미사일, 투사체 등에 사용
    /// 상태머신 없음
    /// </summary>
    public class PassiveObject : CollisionObject
    {
        // 속도 (유닛/초)
        public float velocity { get; set; } = 0f;

        // 이동 방향 (정규화된 벡터)
        public Vector3 direction { get; set; } = Vector3.zero;

        public PassiveObject()
        {
            poolKey = ObjectType.Projectile;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            velocity = 0f;
            direction = Vector3.zero;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            velocity = 0f;
            direction = Vector3.zero;
        }

        /// <summary>
        /// mainProc - 위치 업데이트
        /// </summary>
        public override void mainProc(float dt)
        {
            base.mainProc(dt);

            // 위치 이동: position += direction * velocity * dt
            position += direction * velocity * dt;
        }

        /// <summary>
        /// 방향과 속도 설정
        /// </summary>
        public void SetVelocity(Vector3 moveDirection, float speed)
        {
            direction = moveDirection.normalized;
            velocity = speed;
        }

        /// <summary>
        /// 방향 설정 (속도는 현재 유지)
        /// </summary>
        public void SetDirection(Vector3 newDirection)
        {
            direction = newDirection.normalized;
        }

        /// <summary>
        /// 속도 설정 (방향은 현재 유지)
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            velocity = newSpeed;
        }

        /// <summary>
        /// 이동 정지
        /// </summary>
        public void Stop()
        {
            velocity = 0f;
        }
    }
}
