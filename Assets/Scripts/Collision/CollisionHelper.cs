using Unity.Mathematics;
using Unity.Burst;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌 판정 헬퍼 (순수 수학, Burst 컴파일 최적화)
    /// Job 내부에서 직접 호출되는 static 메서드들
    /// </summary>
    [BurstCompile]
    public static class CollisionHelper
    {
        /// <summary>
        /// 두 충돌체의 교차 판정 진입점
        /// shape 조합에 따라 적절한 알고리즘 선택
        /// </summary>
        public static bool Overlaps(float3 posA, in LogicColliderData a,
                                     float3 posB, in LogicColliderData b)
        {
            return (a.shape, b.shape) switch
            {
                (ColliderShape.AABB,   ColliderShape.AABB)   => OverlapAABB(posA, a.size, posB, b.size),
                (ColliderShape.Circle, ColliderShape.Circle) => OverlapCircle(posA, a.size.x, posB, b.size.x),
                (ColliderShape.AABB,   ColliderShape.Circle) => OverlapAABBvsCircle(posA, a.size, posB, b.size.x),
                (ColliderShape.Circle, ColliderShape.AABB)   => OverlapAABBvsCircle(posB, b.size, posA, a.size.x),
                _ => false,
            };
        }

        /// <summary>
        /// AABB vs AABB 교차 판정 (축정렬 사각형)
        /// 반크기를 이용한 판정 (중심 기준)
        /// </summary>
        private static bool OverlapAABB(float3 posA, float3 sizeA, float3 posB, float3 sizeB)
        {
            var d = math.abs(posA - posB);
            return d.x < sizeA.x + sizeB.x &&
                   d.y < sizeA.y + sizeB.y;
        }

        /// <summary>
        /// Circle vs Circle 교차 판정 (원형)
        /// 거리 제곱 비교 (sqrt 최적화)
        /// </summary>
        private static bool OverlapCircle(float3 posA, float rA, float3 posB, float rB)
        {
            float3 d = posA - posB;
            float r = rA + rB;
            return d.x * d.x + d.y * d.y < r * r; // sqrt 안 씀 (성능 최적화)
        }

        /// <summary>
        /// AABB vs Circle 교차 판정
        /// 박스의 가장 가까운 점과 원의 중심 거리 비교
        /// </summary>
        private static bool OverlapAABBvsCircle(float3 boxPos, float3 boxSize, float3 circPos, float r)
        {
            // 박스 내부에서 원 중심에 가장 가까운 점 찾기
            float3 closest = math.clamp(circPos, boxPos - boxSize, boxPos + boxSize);
            float3 d = circPos - closest;
            return d.x * d.x + d.y * d.y < r * r;
        }

        // Capsule 등 추가 충돌 형태는 필요시 구현
    }
}
