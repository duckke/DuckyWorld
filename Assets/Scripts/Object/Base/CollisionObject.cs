using UnityEngine;
using Unity.Mathematics;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌 + HP 시스템을 가진 오브젝트
    /// 충돌 감지, 데미지, 사망 처리
    /// </summary>
    public class CollisionObject : ObjectBase
    {
        // HP 시스템
        public int hp { get; set; }
        public int maxHp { get; set; }

        // 충돌 레이어 및 마스크 (비트 마스크)
        public CollisionLayer collisionLayer { get; set; } = CollisionLayer.None;
        public int collisionMask { get; set; } = 0;

        // 충돌체 데이터 (애니메이션별 프레임별 충돌체 정보)
        // 로딩 시 1회 할당, 불변
        public AnimationColliderData colliderDataRef { get; set; }

        // 현재 활성 충돌체 범위 (GC 제로: 배열 복사 대신 인덱스만 관리)
        public int activeColliderIndex { get; set; } = 0;
        public int activeColliderCount { get; set; } = 0;

        // 방향 (1=우, -1=좌)
        public int facingDirection { get; set; } = 1;

        public CollisionObject()
        {
            hp = 100;
            maxHp = 100;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            if (hp <= 0) hp = maxHp;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            hp = maxHp;
            facingDirection = 1;
            activeColliderIndex = 0;
            activeColliderCount = 0;
        }

        /// <summary>
        /// 프레임 변경 시 활성 충돌체 범위 업데이트
        /// LogicAnimator에서 프레임 변경되었을 때 호출
        /// </summary>
        public void UpdateActiveColliderRange(int animId, int frameIndex)
        {
            if (colliderDataRef == null) return;

            // AnimationColliderData.frameLookup[animId][frameIndex]에서 범위 조회
            var range = colliderDataRef.GetColliderRange(animId, frameIndex);
            activeColliderIndex = range.startIndex;
            activeColliderCount = range.count;
        }

        /// <summary>
        /// 데미지 처리
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <returns>사망했으면 true</returns>
        public virtual bool TakeDamage(int amount)
        {
            // hp=0인 오브젝트는 파괴 불가 (장애물 등)
            if (maxHp == 0) return false;

            hp -= amount;
            if (hp <= 0)
            {
                hp = 0;
                OnDeath();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 사망 처리 (오버라이드 가능)
        /// </summary>
        public virtual void OnDeath()
        {
            // 회수 예약은 ModuleBase 레벨에서 처리
        }

        /// <summary>
        /// 충돌 처리 (다른 오브젝트와의 충돌)
        /// </summary>
        /// <param name="other">충돌한 다른 오브젝트</param>
        /// <param name="boxType">이 오브젝트의 충돌체 타입</param>
        /// <param name="otherValue">상대방 충돌체의 값 (공격력, 가드 수치 등)</param>
        public virtual void OnCollision(CollisionObject other, ColliderBoxType boxType, int otherValue)
        {
            // 오버라이드 필요
        }

        /// <summary>
        /// 충돌 검사용 스냅샷 생성
        /// CollisionManager.CheckCollisions에서 사용
        /// </summary>
        /// <param name="objectIndex">objects[] 배열 내 인덱스</param>
        /// <returns>스냅샷</returns>
        public ColliderSnapshot BuildSnapshot(int objectIndex)
        {
            return new ColliderSnapshot
            {
                objectIndex = objectIndex,
                position = (float3)position,
                facingDirection = facingDirection,
                collisionLayer = (int)collisionLayer,
                collisionMask = collisionMask,
                colliderStartIndex = activeColliderIndex,
                colliderCount = activeColliderCount,
            };
        }

        /// <summary>
        /// HP 비율 (0 ~ 1)
        /// </summary>
        public float GetHpRatio()
        {
            if (maxHp <= 0) return 1f;
            return (float)hp / maxHp;
        }

        /// <summary>
        /// HP 회복
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (maxHp <= 0) return;
            hp = Mathf.Min(hp + amount, maxHp);
        }
    }

}
