using System.Collections.Generic;
using UnityEngine;
using DuckyWorld.Object;

namespace DuckyWorld.Core
{
    /// <summary>
    /// 모든 로직 모듈의 기본 클래스
    /// GameManager에 등록되어 프레임 루프(preProc→mainProc→postProc)에 참여
    /// ObjectBase 리스트 관리 기능 포함
    /// 충돌 시스템 통합 (CollisionManager)
    /// </summary>
    public abstract class ModuleBase : MonoBehaviour
    {
        [SerializeField] protected string moduleName = "";

        protected bool _isInitialized = false;

        // 로직 타입 오브젝트 레지스트리 (타입별로 빠르게 접근)
        protected Dictionary<System.Type, List<MonoBehaviour>> _registry =
            new Dictionary<System.Type, List<MonoBehaviour>>();

        // Object 계층 리스트 (로직 오브젝트 관리)
        protected List<ObjectBase> _objects = new List<ObjectBase>();

        // 충돌 관리자 (Burst Job 기반)
        protected CollisionManager _collisionManager;
        protected LogicColliderData[] _colliderTable = new LogicColliderData[0];

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                moduleName = this.GetType().Name;
            }
        }

        /// <summary>
        /// 모듈 초기화 - GameManager에서 호출
        /// </summary>
        public virtual void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // 충돌 관리자 초기화
            _collisionManager = new CollisionManager();
            _collisionManager.Init(_colliderTable, 1024); // maxObjects = 1024

            Debug.Log($"[{moduleName}] Initialized");
        }

        /// <summary>
        /// 모듈 정리 - 게임 종료 시 호출
        /// </summary>
        public virtual void Cleanup()
        {
            _isInitialized = false;
            _registry.Clear();

            // 충돌 관리자 정리
            if (_collisionManager != null)
            {
                _collisionManager.Dispose();
                _collisionManager = null;
            }

            Debug.Log($"[{moduleName}] Cleaned up");
        }

        /// <summary>
        /// preProc - 입력 처리, 상태 갱신 전 준비 (각 프레임 시작)
        /// </summary>
        public virtual void DoPreProc()
        {
            // Object 계층 preProc 체인
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                _objects[i].preProc(Time.deltaTime);
            }
        }

        /// <summary>
        /// mainProc - 로직 업데이트 (위치 이동, 상태 변경 등)
        /// 충돌 감지는 이 단계에서 Job으로 스케줄됨 (결과 처리는 postProc에서)
        /// </summary>
        public virtual void DoMainProc()
        {
            // Object 계층 mainProc 체인
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                _objects[i].mainProc(Time.deltaTime);
            }

            // 충돌 감지 (Job 스케줄링)
            if (_collisionManager != null)
            {
                _collisionManager.CheckCollisions(_objects);
            }
        }

        /// <summary>
        /// postProc - 충돌 처리, 뷰 동기화 (프레임 종료)
        /// 충돌 감지 Job 완료 대기 후 결과 처리
        /// </summary>
        public virtual void DoPostProc()
        {
            // 충돌 결과 처리 (Job 완료 대기)
            if (_collisionManager != null)
            {
                _collisionManager.ProcessResults(_objects);
            }

            // Object 계층 postProc 체인
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                _objects[i].postProc(Time.deltaTime);
            }
        }

        /// <summary>
        /// 타입별 오브젝트 등록 (빠른 조회용)
        /// </summary>
        public virtual void RegisterObject<T>(T obj) where T : MonoBehaviour
        {
            if (obj == null) return;

            System.Type type = typeof(T);
            if (!_registry.ContainsKey(type))
            {
                _registry[type] = new List<MonoBehaviour>();
            }

            if (!_registry[type].Contains(obj))
            {
                _registry[type].Add(obj);
            }
        }

        /// <summary>
        /// 타입별 오브젝트 등록 해제
        /// </summary>
        public virtual void UnregisterObject<T>(T obj) where T : MonoBehaviour
        {
            if (obj == null) return;

            System.Type type = typeof(T);
            if (_registry.ContainsKey(type))
            {
                _registry[type].Remove(obj);
            }
        }

        /// <summary>
        /// 특정 타입의 등록된 모든 오브젝트 조회
        /// </summary>
        public virtual List<T> GetRegisteredObjects<T>() where T : MonoBehaviour
        {
            System.Type type = typeof(T);
            List<T> result = new List<T>();

            if (_registry.ContainsKey(type))
            {
                foreach (MonoBehaviour obj in _registry[type])
                {
                    if (obj is T typedObj)
                    {
                        result.Add(typedObj);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// ObjectBase 등록 (로직 오브젝트 관리)
        /// </summary>
        public virtual void RegisterObject(ObjectBase obj)
        {
            if (obj == null) return;
            if (!_objects.Contains(obj))
            {
                _objects.Add(obj);
            }
        }

        /// <summary>
        /// ObjectBase 등록 해제
        /// </summary>
        public virtual void UnregisterObject(ObjectBase obj)
        {
            if (obj == null) return;
            _objects.Remove(obj);
        }

        /// <summary>
        /// 특정 타입의 ObjectBase 모두 조회
        /// </summary>
        public virtual List<T> GetObjects<T>() where T : ObjectBase
        {
            List<T> result = new List<T>();
            foreach (ObjectBase obj in _objects)
            {
                if (obj is T typedObj)
                {
                    result.Add(typedObj);
                }
            }
            return result;
        }

        /// <summary>
        /// 등록된 ObjectBase 개수
        /// </summary>
        public int GetObjectCount() => _objects.Count;

        public bool IsInitialized => _isInitialized;
    }
}
