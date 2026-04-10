using UnityEngine;
using DuckyWorld.Input;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 플레이어 캐릭터 기본 클래스
    /// ActiveObject를 상속하여 상태머신, 조작, 장비 등 지원
    /// </summary>
    public class CharacterBase : ActiveObject
    {
        // 캐릭터 정보 (레벨, 경험치, 체력 등)
        public CharacterInfo characterInfo { get; set; }

        public CharacterBase()
        {
            poolKey = ObjectType.Character;
            characterInfo = new CharacterInfo();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            // 캐릭터 초기화
            if (characterInfo != null)
            {
                characterInfo.Reset();
                hp = characterInfo.maxHealth;
                maxHp = characterInfo.maxHealth;
            }
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            // 캐릭터 정리
        }

        /// <summary>
        /// 입력 처리
        /// 상속 클래스에서 구체적인 움직임 로직 구현
        /// </summary>
        public override void ApplyInput(InputData inputData)
        {
            // 오버라이드 필요
            // 예: 이동, 점프, 공격 등의 입력 처리
        }

        /// <summary>
        /// 경험치 획득
        /// </summary>
        public virtual void GainExperience(int xpAmount)
        {
            if (characterInfo == null) return;

            characterInfo.currentXp += xpAmount;

            // 레벨업 체크
            while (characterInfo.currentXp >= characterInfo.xpToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// 레벨업 처리
        /// </summary>
        protected virtual void LevelUp()
        {
            if (characterInfo == null) return;

            characterInfo.currentXp -= characterInfo.xpToNextLevel;
            characterInfo.level++;

            // 스탯 증가 (상속 클래스에서 오버라이드)
            characterInfo.maxHealth += 10;
            hp = characterInfo.maxHealth;
            maxHp = characterInfo.maxHealth;

            moveSpeed += 0.5f;
            attackPower += 2f;
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public override void Heal(int amount)
        {
            base.Heal(amount);
            if (characterInfo != null)
            {
                characterInfo.currentHealth = hp;
            }
        }

        /// <summary>
        /// 데미지 처리
        /// </summary>
        public override bool TakeDamage(int amount)
        {
            bool isDead = base.TakeDamage(amount);
            if (characterInfo != null)
            {
                characterInfo.currentHealth = hp;
            }

            return isDead;
        }
    }

    /// <summary>
    /// 캐릭터 정보 (런타임 데이터)
    /// </summary>
    public class CharacterInfo
    {
        public int level = 1;
        public int currentXp = 0;
        public int xpToNextLevel = 100;

        public int maxHealth = 100;
        public int currentHealth = 100;

        public int stamina = 50;
        public int maxStamina = 50;

        public float attackSpeed = 1.0f;

        public CharacterInfo()
        {
            Reset();
        }

        public void Reset()
        {
            level = 1;
            currentXp = 0;
            xpToNextLevel = 100;
            maxHealth = 100;
            currentHealth = 100;
            stamina = 50;
            maxStamina = 50;
            attackSpeed = 1.0f;
        }

        /// <summary>
        /// 현재 체력 비율 (0 ~ 1)
        /// </summary>
        public float GetHealthRatio()
        {
            if (maxHealth <= 0) return 1f;
            return (float)currentHealth / maxHealth;
        }

        /// <summary>
        /// 레벨업에 필요한 경험치 계산 (선형)
        /// </summary>
        public void CalculateXpRequirement()
        {
            xpToNextLevel = 100 + (level - 1) * 50;  // 단순 선형 증가
        }
    }
}
