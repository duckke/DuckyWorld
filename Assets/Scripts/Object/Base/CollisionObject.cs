using UnityEngine;

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
        public virtual void OnCollision(CollisionObject other)
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
                position = position,
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

    /// <summary>
    /// 충돌 레이어 (비트 마스크)
    /// </summary>
    public enum CollisionLayer
    {
        None = 0,
        Character = 1 << 0,   // 1
        Monster = 1 << 1,     // 2
        Obstacle = 1 << 2,    // 4
        Projectile = 1 << 3,  // 8
        Item = 1 << 4,        // 16
    }

    /// <summary>
    /// 충돌체 데이터 (struct, 불변)
    /// GC 제로 설계를 위해 모든 필드는 기본형
    /// </summary>
    public struct LogicColliderData
    {
        public ColliderBoxType boxType;    // Attack, Damage, Guard
        public ColliderShape shape;        // AABB, Circle, Capsule
        public Vector3 offset;             // 오브젝트 기준 상대 좌표
        public Vector3 size;               // 반크기 (AABB) 또는 반지름 (Circle)
        public int value;                  // 공격력, 가드 수치 등
    }

    /// <summary>
    /// 충돌체 타입
    /// </summary>
    public enum ColliderBoxType
    {
        Attack = 0,   // 공격 충돌체
        Damage = 1,   // 피해 충돌체 (받는 쪽)
        Guard = 2,    // 방어 충돌체
    }

    /// <summary>
    /// 충돌체 모양
    /// </summary>
    public enum ColliderShape
    {
        AABB = 0,      // 축정렬 사각형
        Circle = 1,    // 원
        Capsule = 2,   // 캡슐 (캡슐)
    }

    /// <summary>
    /// 충돌체 범위 (인덱스와 개수)
    /// </summary>
    public struct ColliderRange
    {
        public int startIndex;
        public int count;

        public ColliderRange(int startIndex, int count)
        {
            this.startIndex = startIndex;
            this.count = count;
        }
    }

    /// <summary>
    /// 애니메이션별 프레임별 충돌체 매핑
    /// 모든 충돌체는 flat 배열에 저장, frameLookup으로 인덱싱
    /// GC 제로 설계
    /// </summary>
    public class AnimationColliderData
    {
        // 전체 충돌체 배열 (flat, 불변)
        public LogicColliderData[] allColliders;

        // 인덱스 룩업: [animId][frameIndex] → (startIndex, count)
        // 동적 애니메이션은 런타임에 빌드, 고정 애니메이션은 에디터에서 사전 구성
        public ColliderRange[][] frameLookup;

        public AnimationColliderData()
        {
            allColliders = new LogicColliderData[0];
            frameLookup = new ColliderRange[0][];
        }

        /// <summary>
        /// 특정 애니메이션의 특정 프레임 충돌체 범위 조회
        /// </summary>
        public ColliderRange GetColliderRange(int animId, int frameIndex)
        {
            if (animId < 0 || animId >= frameLookup.Length) return new ColliderRange(0, 0);
            if (frameIndex < 0 || frameIndex >= frameLookup[animId].Length) return new ColliderRange(0, 0);
            return frameLookup[animId][frameIndex];
        }
    }

    /// <summary>
    /// 충돌 검사용 스냅샷 (Burst Job에 전달)
    /// 클래스 참조 없음, 모두 기본형/struct
    /// </summary>
    public struct ColliderSnapshot
    {
        public int objectIndex;           // objects[] 인덱스 (결과 처리용)
        public Vector3 position;
        public int facingDirection;
        public int collisionLayer;
        public int collisionMask;
        public int colliderStartIndex;    // allColliders 인덱스
        public int colliderCount;
    }

    /// <summary>
    /// 충돌 결과 (Job 출력)
    /// </summary>
    public struct CollisionResult
    {
        public int indexA;               // ColliderSnapshot.objectIndex
        public int indexB;
        public ColliderBoxType boxTypeA;
        public ColliderBoxType boxTypeB;
        public int valueA;
        public int valueB;
    }
}
