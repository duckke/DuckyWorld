using System.Collections.Generic;
using UnityEngine;
using DuckyWorld.Object;

namespace DuckyWorld.Core
{
    /// <summary>
    /// View 전체 관리 - Logic(ObjectBase) → View(ViewObjectBase) 동기화
    /// GameManager.DoUpdate 맨 끝에서 SyncAll 호출
    /// SetEnabled(false)로 View를 완전히 끌 수 있음 → Logic 100% 동일, 화면만 꺼짐 (ML 초고속 시뮬 등)
    /// </summary>
    public class ViewObjectManager : MonoSingleton<ViewObjectManager>
    {
        // objectId → ViewObjectBase 매핑
        private Dictionary<int, ViewObjectBase> _activeViews = new Dictionary<int, ViewObjectBase>();

        // View 활성화 여부 (false = 화면 꺼짐, Logic은 그대로 동작)
        private bool _isEnabled = true;

        // 고아 View 탐지용 HashSet (GC 최소화: 매 프레임 재사용)
        private HashSet<int> _currentLogicIds = new HashSet<int>();

        // 회수 예약 목록 (반복 중 컬렉션 수정 방지)
        private List<int> _orphanIds = new List<int>();

        /// <summary>
        /// View 시스템 활성화/비활성화
        /// false로 설정하면 모든 View가 즉시 숨겨지고 이후 SyncAll에서 스킵됨
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;
            _isEnabled = enabled;

            if (!enabled)
                HideAllViews();
        }

        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// Logic → View 동기화 (GameManager.DoUpdate 맨 끝에서 호출)
        /// View가 비활성화 상태이면 즉시 반환 (Logic에는 영향 없음)
        /// </summary>
        public void SyncAll(List<ObjectBase> objects)
        {
            if (!_isEnabled) return;

            _currentLogicIds.Clear();

            for (int i = 0; i < objects.Count; i++)
            {
                var logic = objects[i];
                _currentLogicIds.Add(logic.objectId);

                // View 없으면 풀에서 생성
                if (!_activeViews.TryGetValue(logic.objectId, out var view))
                {
                    view = PoolManager.Instance?.GetView(logic.poolKey);
                    if (view == null) continue;  // 풀이 없으면 스킵

                    view.OnGet();
                    _activeViews[logic.objectId] = view;
                }

                // 값이 다른 것만 갱신 (불필요한 SetActive/transform 갱신 방지)
                if (view.position != logic.position)
                    view.SetPosition(logic.position);

                if (view.rotation != logic.rotation)
                    view.SetRotation(logic.rotation);

                if (view.currentAnimId != logic.logicAnimator.CurrentAnimId ||
                    view.currentFrame != logic.logicAnimator.CurrentFrame)
                    view.SetAnimation(logic.logicAnimator.CurrentAnimId, logic.logicAnimator.CurrentFrame);

                if (view.facingDirection != logic.facingDirection)
                    view.SetFacing(logic.facingDirection);

                if (view.isVisible != logic.isVisible)
                    view.SetVisible(logic.isVisible);
            }

            // Logic에서 사라진 오브젝트의 View 회수
            CleanupOrphanedViews();
        }

        /// <summary>
        /// 특정 오브젝트의 View를 즉시 회수
        /// Logic 오브젝트 풀 반환 시 함께 호출
        /// </summary>
        public void DestroyView(int objectId)
        {
            if (_activeViews.Remove(objectId, out var view))
            {
                view.OnReturn();
                PoolManager.Instance?.ReturnView(view);
            }
        }

        /// <summary>
        /// Logic 목록에 없는 View 회수 (고아 View 정리)
        /// </summary>
        private void CleanupOrphanedViews()
        {
            _orphanIds.Clear();

            foreach (var kvp in _activeViews)
            {
                if (!_currentLogicIds.Contains(kvp.Key))
                    _orphanIds.Add(kvp.Key);
            }

            for (int i = 0; i < _orphanIds.Count; i++)
            {
                DestroyView(_orphanIds[i]);
            }
        }

        /// <summary>
        /// 모든 활성 View를 숨김 (SetEnabled(false) 시 호출)
        /// View는 유지하되 화면에서만 제거 → 재활성화 시 빠르게 복원 가능
        /// </summary>
        private void HideAllViews()
        {
            foreach (var view in _activeViews.Values)
            {
                view.SetVisible(false);
            }
        }

        /// <summary>
        /// 모든 활성 View 풀 반환 (씬 전환 시 호출)
        /// </summary>
        public void ClearAll()
        {
            _orphanIds.Clear();
            foreach (var kvp in _activeViews)
                _orphanIds.Add(kvp.Key);

            for (int i = 0; i < _orphanIds.Count; i++)
                DestroyView(_orphanIds[i]);

            _activeViews.Clear();
            _currentLogicIds.Clear();
        }

        protected override void OnDestroy()
        {
            ClearAll();
            base.OnDestroy();
        }
    }
}
