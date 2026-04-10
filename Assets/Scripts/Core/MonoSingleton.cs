using UnityEngine;

namespace DuckyWorld.Core
{
    /// <summary>
    /// MonoBehaviour 기반 싱글톤 클래스
    /// Generic으로 상속 클래스를 지정하면 자동으로 인스턴스 관리
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        Debug.LogError($"[MonoSingleton] Instance of {typeof(T).Name} not found in scene!");
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T).Name} detected. Destroying...");
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
