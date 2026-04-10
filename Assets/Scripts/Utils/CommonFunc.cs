using UnityEngine;

namespace DuckyWorld.Utils
{
    /// <summary>
    /// 프로젝트 전역 유틸리티 함수
    /// </summary>
    public static class CommonFunc
    {
        /// <summary>
        /// 값이 범위 내인지 확인
        /// </summary>
        public static bool InRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 값을 범위 내로 제한
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        /// <summary>
        /// 두 벡터 간 거리 계산 (거리 제곱 - 루트 계산 최소화)
        /// </summary>
        public static float DistanceSqr(Vector2 a, Vector2 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// 두 벡터 간 거리 계산
        /// </summary>
        public static float Distance(Vector2 a, Vector2 b)
        {
            return Mathf.Sqrt(DistanceSqr(a, b));
        }

        /// <summary>
        /// 오브젝트 풀에서 받은 오브젝트 초기화 (회전/스케일 리셋)
        /// </summary>
        public static void ResetTransform(Transform tr, Vector3 position, Quaternion rotation = default)
        {
            tr.position = position;
            tr.rotation = rotation == default ? Quaternion.identity : rotation;
            tr.localScale = Vector3.one;
        }

        /// <summary>
        /// Safe GetComponent - 없으면 로그 출력 후 null 반환
        /// </summary>
        public static T SafeGetComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                Debug.LogWarning($"[CommonFunc] {typeof(T).Name} not found on {go.name}");
            }
            return component;
        }

        /// <summary>
        /// 각도를 -180 ~ 180 범위로 정규화
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}
