using UnityEngine;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 장애물 기본 클래스
    /// CollisionObject를 상속하여 고정된 장애물 제공
    /// 기본적으로 파괴 불가능 (hp=0)
    /// 특수 효과 적용 가능 (Stun, SlowDown 등)
    /// </summary>
    public class ObstacleBase : CollisionObject
    {
        // 장애물 효과 타입
        public ObstacleEffectType obstacleEffectType { get; set; } = ObstacleEffectType.None;

        // 효과값 (Stun 시간, SlowDown 비율 등)
        public float effectValue { get; set; } = 0f;

        public ObstacleBase()
        {
            poolKey = ObjectType.Obstacle;

            // 기본값: 파괴 불가능
            hp = 0;
            maxHp = 0;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            // hp=0으로 유지 (파괴 불가능)
            hp = 0;
            maxHp = 0;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            obstacleEffectType = ObstacleEffectType.None;
            effectValue = 0f;
        }

        /// <summary>
        /// 충돌 처리
        /// 다른 오브젝트와 충돌했을 때 효과 적용
        /// </summary>
        public override void OnCollision(CollisionObject other)
        {
            if (other == null || other is not ActiveObject activeObj) return;

            ApplyObstacleEffect(activeObj);
        }

        /// <summary>
        /// 장애물 효과 적용
        /// </summary>
        private void ApplyObstacleEffect(ActiveObject target)
        {
            switch (obstacleEffectType)
            {
                case ObstacleEffectType.None:
                    // 효과 없음
                    break;

                case ObstacleEffectType.Instant:
                    // 즉시 데미지
                    ApplyInstantDamage(target);
                    break;

                case ObstacleEffectType.Stun:
                    // 기절 효과
                    ApplyStunEffect(target);
                    break;

                case ObstacleEffectType.SlowDown:
                    // 감속 효과
                    ApplySlowDownEffect(target);
                    break;

                case ObstacleEffectType.SpeedUp:
                    // 가속 효과
                    ApplySpeedUpEffect(target);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 즉시 데미지 적용
        /// </summary>
        private void ApplyInstantDamage(ActiveObject target)
        {
            int damage = (int)effectValue;
            if (damage > 0)
            {
                target.TakeDamage(damage);
            }
        }

        /// <summary>
        /// 기절 효과 적용
        /// </summary>
        private void ApplyStunEffect(ActiveObject target)
        {
            float stunDuration = effectValue;
            if (stunDuration > 0)
            {
                var stun = new Appendage(1, AppendageType.Stun, stunDuration, 0f);
                target.AddAppendage(stun);
            }
        }

        /// <summary>
        /// 감속 효과 적용
        /// </summary>
        private void ApplySlowDownEffect(ActiveObject target)
        {
            float slowDuration = 5f;  // 5초
            float slowValue = effectValue;  // 속도 감소율 (0.5 = 50% 감소)

            if (slowValue > 0)
            {
                var slow = new Appendage(2, AppendageType.SpeedDown, slowDuration, slowValue);
                target.AddAppendage(slow);
            }
        }

        /// <summary>
        /// 가속 효과 적용 (특수 장애물)
        /// </summary>
        private void ApplySpeedUpEffect(ActiveObject target)
        {
            float speedUpDuration = 3f;  // 3초
            float speedUpValue = effectValue;  // 속도 증가값

            if (speedUpValue > 0)
            {
                var speedUp = new Appendage(3, AppendageType.SpeedUp, speedUpDuration, speedUpValue);
                target.AddAppendage(speedUp);
            }
        }

        /// <summary>
        /// 데미지 무시 (장애물은 파괴 불가)
        /// </summary>
        public override bool TakeDamage(int amount)
        {
            // hp=0이므로 항상 false
            return false;
        }

        /// <summary>
        /// 회복 무시 (필요 없음)
        /// </summary>
        public override void Heal(int amount)
        {
            // 장애물은 회복 불가
        }
    }

    /// <summary>
    /// 장애물 효과 타입
    /// </summary>
    public enum ObstacleEffectType
    {
        None = 0,       // 효과 없음
        Instant = 1,    // 즉시 데미지 (effectValue = 데미지)
        Stun = 2,       // 기절 (effectValue = 지속시간)
        SlowDown = 3,   // 감속 (effectValue = 감속 비율)
        SpeedUp = 4,    // 가속 (effectValue = 가속값, 특수)
    }
}
