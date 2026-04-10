using System.Collections.Generic;
using UnityEngine;
using DuckyWorld.Utils;

namespace DuckyWorld.Core
{
    /// <summary>
    /// 오브젝트 풀 관리자 - 동적 메모리 할당 최소화
    /// Prefab 기반 풀 생성, Get/Return 처리
    /// </summary>
    public class PoolManager : MonoSingleton<PoolManager>
    {
        [System.Serializable]
        public class PoolConfig
        {
            public string poolName;
            public GameObject prefab;
            public int initialSize = CommonConsts.DEFAULT_POOL_SIZE;
            public int prewarmCount = CommonConsts.DEFAULT_POOL_PREWARM;
        }

        [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();

        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private Dictionary<GameObject, string> _poolMap = new Dictionary<GameObject, string>();
        private Transform _poolRoot;

        protected override void Awake()
        {
            base.Awake();

            // 풀 루트 오브젝트 생성
            _poolRoot = transform.Find("PoolRoot");
            if (_poolRoot == null)
            {
                GameObject rootGo = new GameObject("PoolRoot");
                rootGo.transform.SetParent(transform);
                _poolRoot = rootGo.transform;
            }
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 모든 풀 초기화 및 PreWarm
        /// </summary>
        public void Initialize()
        {
            foreach (PoolConfig config in _poolConfigs)
            {
                CreatePool(config.poolName, config.prefab, config.initialSize);
                PreWarm(config.poolName, config.prewarmCount);
            }
            Debug.Log("[PoolManager] Initialized");
        }

        /// <summary>
        /// 새 풀 생성 (동적 등록용)
        /// </summary>
        public void CreatePool(string poolName, GameObject prefab, int initialSize = 0)
        {
            if (_pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists!");
                return;
            }

            if (prefab == null)
            {
                Debug.LogError($"[PoolManager] Prefab for pool '{poolName}' is null!");
                return;
            }

            _pools[poolName] = new Queue<GameObject>(initialSize);
            _prefabs[poolName] = prefab;

            Debug.Log($"[PoolManager] Pool '{poolName}' created with capacity {initialSize}");
        }

        /// <summary>
        /// 풀 사전 준비 (initialSize만큼 미리 생성)
        /// </summary>
        public void PreWarm(string poolName, int count)
        {
            if (!_pools.ContainsKey(poolName))
            {
                Debug.LogError($"[PoolManager] Pool '{poolName}' does not exist!");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject obj = CreateInstance(poolName);
                obj.SetActive(false);
                _pools[poolName].Enqueue(obj);
            }

            Debug.Log($"[PoolManager] PreWarmed '{poolName}' with {count} objects");
        }

        /// <summary>
        /// 풀에서 오브젝트 꺼내기
        /// </summary>
        public GameObject Get(string poolName)
        {
            if (!_pools.ContainsKey(poolName))
            {
                Debug.LogError($"[PoolManager] Pool '{poolName}' does not exist!");
                return null;
            }

            GameObject obj;
            if (_pools[poolName].Count > 0)
            {
                obj = _pools[poolName].Dequeue();
            }
            else
            {
                obj = CreateInstance(poolName);
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// 풀에 오브젝트 반환
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            if (!_poolMap.ContainsKey(obj))
            {
                Debug.LogWarning($"[PoolManager] Object '{obj.name}' not registered in pool map!");
                Destroy(obj);
                return;
            }

            string poolName = _poolMap[obj];
            obj.SetActive(false);
            obj.transform.SetParent(_poolRoot);
            _pools[poolName].Enqueue(obj);
        }

        /// <summary>
        /// 인스턴스 생성 (내부 헬퍼)
        /// </summary>
        private GameObject CreateInstance(string poolName)
        {
            if (!_prefabs.ContainsKey(poolName))
            {
                Debug.LogError($"[PoolManager] Prefab for pool '{poolName}' not found!");
                return null;
            }

            GameObject obj = Instantiate(_prefabs[poolName], _poolRoot);
            obj.name = _prefabs[poolName].name; // 프리팹 이름으로 설정
            _poolMap[obj] = poolName;

            return obj;
        }

        /// <summary>
        /// 특정 풀 상태 조회
        /// </summary>
        public int GetPoolCount(string poolName)
        {
            if (_pools.ContainsKey(poolName))
            {
                return _pools[poolName].Count;
            }
            return -1;
        }

        /// <summary>
        /// 모든 풀 초기화
        /// </summary>
        public void ClearAll()
        {
            foreach (var poolQueue in _pools.Values)
            {
                foreach (var obj in poolQueue)
                {
                    Destroy(obj);
                }
                poolQueue.Clear();
            }
            _poolMap.Clear();
            Debug.Log("[PoolManager] All pools cleared");
        }

        protected override void OnDestroy()
        {
            ClearAll();
            base.OnDestroy();
        }
    }
}
