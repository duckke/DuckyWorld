using System.Collections.Generic;
using UnityEngine;
using DuckyWorld.Utils;
using DuckyWorld.Object;
using DuckyWorld.Factory;

namespace DuckyWorld.Core
{
    /// <summary>
    /// 오브젝트 풀 관리자 - 동적 메모리 할당 최소화
    /// Prefab 기반 풀 생성, Get/Return 처리
    /// View 풀(ViewObjectBase 전용)도 함께 관리
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

        [System.Serializable]
        public class ViewPoolConfig
        {
            public ObjectType objectType;
            public GameObject viewPrefab;
            public int prewarmCount = CommonConsts.DEFAULT_POOL_PREWARM;
        }

        [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();
        [SerializeField] private List<ViewPoolConfig> _viewPoolConfigs = new List<ViewPoolConfig>();

        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private Dictionary<GameObject, string> _poolMap = new Dictionary<GameObject, string>();

        // View 풀 (Logic 풀과 완전히 분리)
        private Dictionary<ObjectType, Queue<ViewObjectBase>> _viewPools = new Dictionary<ObjectType, Queue<ViewObjectBase>>();
        private Dictionary<ObjectType, GameObject> _viewPrefabs = new Dictionary<ObjectType, GameObject>();
        private Dictionary<ViewObjectBase, ObjectType> _viewPoolMap = new Dictionary<ViewObjectBase, ObjectType>();

        private Transform _poolRoot;
        private Transform _viewPoolRoot;

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

            // View 풀 루트 오브젝트 생성
            _viewPoolRoot = transform.Find("ViewPoolRoot");
            if (_viewPoolRoot == null)
            {
                GameObject viewRootGo = new GameObject("ViewPoolRoot");
                viewRootGo.transform.SetParent(transform);
                _viewPoolRoot = viewRootGo.transform;
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

            foreach (ViewPoolConfig config in _viewPoolConfigs)
            {
                CreateViewPool(config.objectType, config.viewPrefab);
                PreWarmView(config.objectType, config.prewarmCount);
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

        // =====================================================================
        // View 풀 (ViewObjectBase 전용 — Logic 풀과 완전히 분리)
        // =====================================================================

        /// <summary>
        /// View 풀 생성 (ObjectType별)
        /// </summary>
        public void CreateViewPool(ObjectType type, GameObject viewPrefab)
        {
            if (_viewPools.ContainsKey(type))
            {
                Debug.LogWarning($"[PoolManager] View pool '{type}' already exists!");
                return;
            }

            if (viewPrefab == null)
            {
                Debug.LogError($"[PoolManager] View prefab for '{type}' is null!");
                return;
            }

            if (viewPrefab.GetComponent<ViewObjectBase>() == null)
            {
                Debug.LogError($"[PoolManager] Prefab '{viewPrefab.name}' has no ViewObjectBase component!");
                return;
            }

            _viewPools[type] = new Queue<ViewObjectBase>();
            _viewPrefabs[type] = viewPrefab;
        }

        /// <summary>
        /// View 풀 사전 준비
        /// </summary>
        public void PreWarmView(ObjectType type, int count)
        {
            if (!_viewPools.ContainsKey(type))
            {
                Debug.LogError($"[PoolManager] View pool '{type}' does not exist!");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                ViewObjectBase view = CreateViewInstance(type);
                view.gameObject.SetActive(false);
                _viewPools[type].Enqueue(view);
            }

            Debug.Log($"[PoolManager] PreWarmed view pool '{type}' with {count} objects");
        }

        /// <summary>
        /// View 풀에서 꺼내기
        /// </summary>
        public ViewObjectBase GetView(ObjectType type)
        {
            if (!_viewPools.ContainsKey(type))
            {
                Debug.LogError($"[PoolManager] View pool '{type}' does not exist!");
                return null;
            }

            ViewObjectBase view;
            if (_viewPools[type].Count > 0)
            {
                view = _viewPools[type].Dequeue();
            }
            else
            {
                view = CreateViewInstance(type);
            }

            view.viewType = type;
            return view;
        }

        /// <summary>
        /// View 풀에 반환
        /// </summary>
        public void ReturnView(ViewObjectBase view)
        {
            if (view == null) return;

            if (!_viewPoolMap.TryGetValue(view, out ObjectType type))
            {
                Debug.LogWarning($"[PoolManager] ViewObjectBase '{view.name}' not registered in view pool map!");
                Destroy(view.gameObject);
                return;
            }

            view.gameObject.SetActive(false);
            view.transform.SetParent(_viewPoolRoot);
            _viewPools[type].Enqueue(view);
        }

        /// <summary>
        /// View 인스턴스 생성 (내부 헬퍼)
        /// </summary>
        private ViewObjectBase CreateViewInstance(ObjectType type)
        {
            if (!_viewPrefabs.TryGetValue(type, out GameObject prefab))
            {
                Debug.LogError($"[PoolManager] View prefab for '{type}' not found!");
                return null;
            }

            GameObject go = Instantiate(prefab, _viewPoolRoot);
            go.name = prefab.name;
            ViewObjectBase view = go.GetComponent<ViewObjectBase>();
            _viewPoolMap[view] = type;
            return view;
        }

        /// <summary>
        /// 특정 View 풀 상태 조회
        /// </summary>
        public int GetViewPoolCount(ObjectType type)
        {
            if (_viewPools.TryGetValue(type, out var queue))
                return queue.Count;
            return -1;
        }

        // =====================================================================
        // 전체 정리
        // =====================================================================

        /// <summary>
        /// 모든 풀 초기화 (씬 전환 시)
        /// </summary>
        public void ClearAll()
        {
            // Logic 풀 정리
            foreach (var poolQueue in _pools.Values)
            {
                foreach (var obj in poolQueue)
                {
                    Destroy(obj);
                }
                poolQueue.Clear();
            }
            _poolMap.Clear();

            // View 풀 정리
            foreach (var viewQueue in _viewPools.Values)
            {
                foreach (var view in viewQueue)
                {
                    if (view != null)
                        Destroy(view.gameObject);
                }
                viewQueue.Clear();
            }
            _viewPoolMap.Clear();

            Debug.Log("[PoolManager] All pools cleared");
        }

        protected override void OnDestroy()
        {
            ClearAll();
            base.OnDestroy();
        }
    }
}
