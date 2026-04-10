using UnityEngine;
using DuckyWorld.Factory;

namespace DuckyWorld.Object
{
    /// <summary>
    /// View 레이어 - 실제 GameObject 표현
    /// Logic(ObjectBase)의 상태를 받아 화면에 반영
    /// ViewObjectManager가 Logic 상태와 동기화를 담당
    /// </summary>
    public class ViewObjectBase : MonoBehaviour
    {
        // 마지막으로 동기화된 Logic 값 (변경 감지용)
        public Vector3 position { get; private set; }
        public Quaternion rotation { get; private set; }
        public int currentAnimId { get; private set; } = -1;
        public int currentFrame { get; private set; } = 0;
        public int facingDirection { get; private set; } = 1;
        public bool isVisible { get; private set; } = true;

        // 풀 반환 시 어떤 타입인지 추적 (ViewPool용)
        public ObjectType viewType { get; set; }

        // 렌더러 참조 (초기화 시 캐싱)
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 풀에서 꺼낼 때 초기화
        /// </summary>
        public virtual void OnGet()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            currentAnimId = -1;
            currentFrame = 0;
            facingDirection = 1;
            isVisible = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 풀에 반환할 때 정리
        /// </summary>
        public virtual void OnReturn()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 위치 동기화
        /// </summary>
        public void SetPosition(Vector3 pos)
        {
            position = pos;
            transform.position = pos;
        }

        /// <summary>
        /// 회전 동기화
        /// </summary>
        public void SetRotation(Quaternion rot)
        {
            rotation = rot;
            transform.rotation = rot;
        }

        /// <summary>
        /// 애니메이션 동기화
        /// SpriteRenderer(스프라이트 시트) 또는 Animator 중 하나 사용
        /// </summary>
        public virtual void SetAnimation(int animId, int frame)
        {
            currentAnimId = animId;
            currentFrame = frame;

            // Animator가 있으면 파라미터로 전달
            if (_animator != null)
            {
                _animator.SetInteger("AnimId", animId);
                _animator.SetInteger("Frame", frame);
                return;
            }

            // SpriteRenderer가 있으면 스프라이트 시트 인덱스로 처리
            // 실제 스프라이트 배열은 하위 클래스에서 오버라이드하여 관리
        }

        /// <summary>
        /// 방향 동기화 (좌우 반전)
        /// </summary>
        public void SetFacing(int dir)
        {
            facingDirection = dir;
            // localScale.x로 반전 (1=우, -1=좌)
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            transform.localScale = scale;
        }

        /// <summary>
        /// 표시 여부 동기화
        /// </summary>
        public void SetVisible(bool visible)
        {
            isVisible = visible;
            // SetActive 대신 렌더러만 끄기 (GameObject 비활성화는 ReturnToPool 시에만)
            if (_spriteRenderer != null)
                _spriteRenderer.enabled = visible;
            else
                gameObject.SetActive(visible);
        }
    }
}
