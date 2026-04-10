namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌 결과 (Job 출력, struct)
    /// Burst Job의 결과로 생성, postProc에서 처리됨
    /// </summary>
    public struct CollisionResult
    {
        public int indexA;               // ColliderSnapshot.objectIndex (오브젝트 배열 인덱스)
        public int indexB;
        public ColliderBoxType boxTypeA; // A의 충돌체 타입
        public ColliderBoxType boxTypeB;
        public int valueA;               // A의 충돌체 값 (공격력, 가드 수치 등)
        public int valueB;
    }
}
