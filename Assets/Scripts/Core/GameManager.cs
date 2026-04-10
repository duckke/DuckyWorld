using System.Collections.Generic;
using UnityEngine;

namespace DuckyWorld.Core
{
    /// <summary>
    /// 게임 매니저 - 전체 게임 루프 관리
    /// 모든 모듈의 프레임 콜백(preProc→mainProc→postProc) 호출 담당
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        private List<ModuleBase> _modules = new List<ModuleBase>();
        private bool _isPaused = false;
        private float _deltaTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            Time.fixedDeltaTime = Utils.CommonConsts.FIXED_DELTA_TIME;
            Application.targetFrameRate = Utils.CommonConsts.TARGET_FPS;
        }

        private void Start()
        {
            InitializeModules();
        }

        private void Update()
        {
            if (_isPaused) return;

            _deltaTime = Time.deltaTime;
            DoUpdate();
        }

        /// <summary>
        /// 메인 업데이트 루프 - 프레임별 실행
        /// preProc → mainProc → postProc 순서로 모든 모듈 호출
        /// </summary>
        private void DoUpdate()
        {
            // Step 1: PreProc - 입력 및 상태 갱신 준비
            foreach (ModuleBase module in _modules)
            {
                if (module.IsInitialized)
                {
                    module.DoPreProc();
                }
            }

            // Step 2: MainProc - 로직 업데이트
            foreach (ModuleBase module in _modules)
            {
                if (module.IsInitialized)
                {
                    module.DoMainProc();
                }
            }

            // Step 3: PostProc - 충돌 처리, 뷰 동기화
            foreach (ModuleBase module in _modules)
            {
                if (module.IsInitialized)
                {
                    module.DoPostProc();
                }
            }

            Debug.Log("[GameManager] Frame updated - DoUpdate called");
        }

        /// <summary>
        /// 모든 모듈 초기화
        /// </summary>
        private void InitializeModules()
        {
            // 자동으로 씬의 모든 ModuleBase 자식 찾기
            ModuleBase[] foundModules = GetComponentsInChildren<ModuleBase>();
            foreach (ModuleBase module in foundModules)
            {
                RegisterModule(module);
            }

            // 명시적으로 추가된 모듈 초기화
            foreach (ModuleBase module in _modules)
            {
                module.Initialize();
            }

            Debug.Log($"[GameManager] Initialized {_modules.Count} modules");
        }

        /// <summary>
        /// 모듈 등록
        /// </summary>
        public void RegisterModule(ModuleBase module)
        {
            if (module == null)
            {
                Debug.LogError("[GameManager] Cannot register null module!");
                return;
            }

            if (_modules.Contains(module))
            {
                Debug.LogWarning($"[GameManager] Module {module.GetType().Name} already registered!");
                return;
            }

            _modules.Add(module);
        }

        /// <summary>
        /// 모듈 등록 해제
        /// </summary>
        public void UnregisterModule(ModuleBase module)
        {
            if (module != null)
            {
                _modules.Remove(module);
            }
        }

        /// <summary>
        /// 게임 일시정지/재개
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            Debug.Log($"[GameManager] Game {(paused ? "paused" : "resumed")}");
        }

        public bool IsPaused => _isPaused;
        public float DeltaTime => _deltaTime;
        public int ModuleCount => _modules.Count;
    }
}
