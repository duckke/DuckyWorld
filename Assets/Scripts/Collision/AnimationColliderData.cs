namespace DuckyWorld.Object
{
    /// <summary>
    /// 애니메이션별 프레임별 충돌체 매핑 (로딩 시 1회 생성, 불변)
    /// GC 제로 설계: flat 배열 + 범위 룩업
    /// </summary>
    public class AnimationColliderData
    {
        // 전체 충돌체 배열 (flat, 모든 애니메이션의 모든 프레임 데이터)
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
}
