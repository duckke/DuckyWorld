using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌 감지 및 처리 관리자 (Burst Job 기반)
    /// mainProc에서 감지, postProc에서 결과 처리
    /// </summary>
    public class CollisionManager
    {
        private NativeArray<ColliderSnapshot> _snapshots;      // 매 프레임마다 채워짐
        private NativeArray<LogicColliderData> _allColliders;  // 로딩 시 1회 할당 (불변)
        private NativeList<CollisionResult> _results;          // Job 출력 결과

        private JobHandle _lastJobHandle;
        private bool _isJobScheduled = false;

        public CollisionManager()
        {
            _snapshots = default;
            _allColliders = default;
            _results = default;
            _lastJobHandle = default;
        }

        /// <summary>
        /// 초기화 (GameManager 시작 시 호출)
        /// </summary>
        /// <param name="colliderTable">모든 충돌체 데이터 (flat 배열)</param>
        /// <param name="maxObjects">동시에 활성화될 수 있는 최대 오브젝트 수</param>
        public void Init(LogicColliderData[] colliderTable, int maxObjects)
        {
            _allColliders = new NativeArray<LogicColliderData>(colliderTable, Allocator.Persistent);
            _snapshots = new NativeArray<ColliderSnapshot>(maxObjects, Allocator.Persistent);
            _results = new NativeList<CollisionResult>(256, Allocator.Persistent);
        }

        /// <summary>
        /// 정리 (게임 종료 시 호출)
        /// </summary>
        public void Dispose()
        {
            // Job이 완료될 때까지 대기
            if (_isJobScheduled)
            {
                _lastJobHandle.Complete();
                _isJobScheduled = false;
            }

            if (_snapshots.IsCreated) _snapshots.Dispose();
            if (_allColliders.IsCreated) _allColliders.Dispose();
            if (_results.IsCreated) _results.Dispose();
        }

        /// <summary>
        /// mainProc에서 호출 — 스냅샷 빌드 후 Job 스케줄링
        /// 이전 Job이 완료될 때까지 대기
        /// </summary>
        public void CheckCollisions(List<ObjectBase> objects)
        {
            // 이전 Job 완료 대기
            if (_isJobScheduled)
            {
                _lastJobHandle.Complete();
                _isJobScheduled = false;
            }

            // 스냅샷 수집 (CollisionObject만 포함)
            int count = 0;
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] is not CollisionObject co) continue;
                if (count >= _snapshots.Length)
                {
                    UnityEngine.Debug.LogWarning($"[CollisionManager] 스냅샷 배열 크기 초과: {count} >= {_snapshots.Length}");
                    break;
                }
                _snapshots[count] = co.BuildSnapshot(i);
                count++;
            }

            // 결과 초기화
            _results.Clear();

            // Job 생성 및 스케줄링
            var job = new CollisionJob
            {
                snapshots = _snapshots.GetSubArray(0, count),
                allColliders = _allColliders,
                results = _results,
            };

            _lastJobHandle = job.Schedule();
            _isJobScheduled = true;
        }

        /// <summary>
        /// postProc에서 호출 — Job 완료 대기 후 결과 처리
        /// </summary>
        public void ProcessResults(List<ObjectBase> objects)
        {
            // Job 완료 대기
            if (_isJobScheduled)
            {
                _lastJobHandle.Complete();
                _isJobScheduled = false;
            }

            // 각 충돌 결과 처리
            for (int i = 0; i < _results.Length; i++)
            {
                var r = _results[i];

                // objects[indexA], objects[indexB] 조회 (주의: BuildSnapshot에서 전달한 objectIndex 사용)
                var a = objects[r.indexA] as CollisionObject;
                var b = objects[r.indexB] as CollisionObject;

                // 양쪽 오브젝트에 충돌 콜백 실행
                a?.OnCollision(b, r.boxTypeA, r.valueB);
                b?.OnCollision(a, r.boxTypeB, r.valueA);
            }
        }
    }

    /// <summary>
    /// Burst 컴파일 충돌 감지 Job
    /// 모든 스냅샷 쌍에 대해 충돌 검사 실행
    /// </summary>
    [BurstCompile]
    public struct CollisionJob : IJob
    {
        [ReadOnly] public NativeArray<ColliderSnapshot> snapshots;
        [ReadOnly] public NativeArray<LogicColliderData> allColliders;
        public NativeList<CollisionResult> results;

        public void Execute()
        {
            // O(n²) 충돌 검사 (BroadPhase는 단순화, NarrowPhase는 충돌 형태별 판정)
            for (int i = 0; i < snapshots.Length; i++)
            {
                for (int j = i + 1; j < snapshots.Length; j++)
                {
                    var a = snapshots[i];
                    var b = snapshots[j];

                    // 레이어/마스크 체크: 한쪽이라도 다른 쪽을 무시하면 스킵
                    if ((a.collisionMask & b.collisionLayer) == 0 &&
                        (b.collisionMask & a.collisionLayer) == 0)
                        continue;

                    // a의 활성 충돌체 vs b의 활성 충돌체
                    for (int ai = a.colliderStartIndex; ai < a.colliderStartIndex + a.colliderCount; ai++)
                    {
                        for (int bi = b.colliderStartIndex; bi < b.colliderStartIndex + b.colliderCount; bi++)
                        {
                            var ca = allColliders[ai];
                            var cb = allColliders[bi];

                            // 월드 좌표로 변환 (offset에 방향 미러링 적용)
                            float3 worldOffsetA = new float3(ca.offset.x * a.facingDirection, ca.offset.y, ca.offset.z);
                            float3 worldOffsetB = new float3(cb.offset.x * b.facingDirection, cb.offset.y, cb.offset.z);

                            float3 worldPosA = a.position + worldOffsetA;
                            float3 worldPosB = b.position + worldOffsetB;

                            // 충돌 판정
                            if (CollisionHelper.Overlaps(worldPosA, ca, worldPosB, cb))
                            {
                                results.Add(new CollisionResult
                                {
                                    indexA = a.objectIndex,
                                    indexB = b.objectIndex,
                                    boxTypeA = ca.boxType,
                                    boxTypeB = cb.boxType,
                                    valueA = ca.value,
                                    valueB = cb.value,
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 충돌 검사용 스냅샷 (Burst Job에 전달)
    /// 클래스 참조 없음, 모두 기본형/struct
    /// </summary>
    public struct ColliderSnapshot
    {
        public int objectIndex;           // objects[] 배열 인덱스 (결과 처리용)
        public float3 position;           // 월드 좌표
        public int facingDirection;       // 1=우, -1=좌
        public int collisionLayer;        // 이 오브젝트의 레이어
        public int collisionMask;         // 이 오브젝트가 충돌할 레이어 마스크
        public int colliderStartIndex;    // allColliders 내 활성 충돌체 시작 인덱스
        public int colliderCount;         // 활성 충돌체 개수
    }
}
