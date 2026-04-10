using Unity.Mathematics;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌체 하나의 데이터 (struct, 불변)
    /// float3 사용 (Unity.Mathematics) — Burst 호환 필수
    /// </summary>
    public struct LogicColliderData
    {
        public ColliderBoxType boxType;   // Attack, Damage, Guard
        public ColliderShape shape;       // AABB, Circle, Capsule
        public float3 offset;             // 오브젝트 기준 상대 좌표
        public float3 size;               // 반크기 (AABB) 또는 x=반지름 (Circle)
        public int value;                 // 공격력, 가드 수치 등
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
        Capsule = 2,   // 캡슐
    }
}
