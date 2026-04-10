using UnityEngine;
using System.Collections.Generic;
using DuckyWorld.Input;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 상태머신 + 조작 가능한 오브젝트
    /// 캐릭터, 몬스터 등
    /// Appendage(버프/디버프) 체인 지원
    /// </summary>
    public class ActiveObject : CollisionObject
    {
        // 움직임 속성
        public float moveSpeed { get; set; } = 5f;
        public float jumpForce { get; set; } = 10f;
        public float attackPower { get; set; } = 10f;

        // 상태 (상속 클래스에서 정의)
        public int currentState { get; set; } = 0;

        // Appendage 관리 (ID별 리스트: 같은 ID의 여러 appendage 가능)
        private Dictionary<int, List<Appendage>> m_appendages = new Dictionary<int, List<Appendage>>();

        public ActiveObject()
        {
            poolKey = ObjectType.Character;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            currentState = 0;
            m_appendages.Clear();
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            currentState = 0;
            RemoveAllAppendages();
        }

        /// <summary>
        /// preProc - 입력 처리, appendage 업데이트
        /// </summary>
        public override void preProc(float dt)
        {
            base.preProc(dt);

            // Appendage preProc 체인
            foreach (var list in m_appendages.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].preProc(dt);
                }
            }
        }

        /// <summary>
        /// mainProc - 로직 업데이트, appendage 지속시간 체크
        /// </summary>
        public override void mainProc(float dt)
        {
            base.mainProc(dt);

            // Appendage mainProc 체인 및 만료 체크
            var expiredIds = new List<int>();
            foreach (var kvp in m_appendages)
            {
                int appendageId = kvp.Key;
                var list = kvp.Value;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!list[i].mainProc(dt))
                    {
                        // 지속시간 만료
                        list[i].OnDetach(this);
                        list.RemoveAt(i);
                    }
                }

                if (list.Count == 0)
                {
                    expiredIds.Add(appendageId);
                }
            }

            // 빈 리스트 정리
            foreach (int id in expiredIds)
            {
                m_appendages.Remove(id);
            }
        }

        /// <summary>
        /// postProc - 뷰 동기화, appendage 마무리
        /// </summary>
        public override void postProc(float dt)
        {
            base.postProc(dt);

            // Appendage postProc 체인
            foreach (var list in m_appendages.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].postProc(dt);
                }
            }
        }

        /// <summary>
        /// Appendage 추가
        /// </summary>
        public void AddAppendage(Appendage appendage)
        {
            if (appendage == null) return;

            int id = appendage.appendageId;
            if (!m_appendages.ContainsKey(id))
            {
                m_appendages[id] = new List<Appendage>();
            }

            m_appendages[id].Add(appendage);
            appendage.OnAttach(this);
        }

        /// <summary>
        /// 특정 ID의 모든 Appendage 제거
        /// </summary>
        public void RemoveAppendagesById(int appendageId)
        {
            if (!m_appendages.ContainsKey(appendageId)) return;

            var list = m_appendages[appendageId];
            for (int i = 0; i < list.Count; i++)
            {
                list[i].OnDetach(this);
            }

            m_appendages.Remove(appendageId);
        }

        /// <summary>
        /// 모든 Appendage 제거
        /// </summary>
        public void RemoveAllAppendages()
        {
            foreach (var list in m_appendages.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].OnDetach(this);
                }
            }

            m_appendages.Clear();
        }

        /// <summary>
        /// 특정 ID의 Appendage 개수 조회
        /// </summary>
        public int GetAppendageCount(int appendageId)
        {
            if (!m_appendages.ContainsKey(appendageId)) return 0;
            return m_appendages[appendageId].Count;
        }

        /// <summary>
        /// 입력 처리 (상속 클래스에서 구현)
        /// </summary>
        /// <param name="inputData">입력 데이터</param>
        public virtual void ApplyInput(InputData inputData)
        {
            // 오버라이드 필요
        }

        /// <summary>
        /// 특정 Appendage 타입 있는지 확인
        /// </summary>
        public bool HasAppendageType(AppendageType type)
        {
            foreach (var list in m_appendages.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].type == type) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 특정 Appendage 타입의 모든 인스턴스 가져오기
        /// </summary>
        public List<Appendage> GetAppendagesByType(AppendageType type)
        {
            var result = new List<Appendage>();
            foreach (var list in m_appendages.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].type == type)
                    {
                        result.Add(list[i]);
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Appendage 타입 (버프/디버프)
    /// </summary>
    public enum AppendageType
    {
        SpeedUp = 0,      // 속도 증가
        SpeedDown = 1,    // 속도 감소
        Invincible = 2,   // 무적
        Stun = 3,         // 기절
        Bleeding = 4,     // 출혈 (지속 데미지)
        Poison = 5,       // 중독
        Barrier = 6,      // 배리어
    }
}
