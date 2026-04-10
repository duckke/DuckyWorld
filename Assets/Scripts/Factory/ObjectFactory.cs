using UnityEngine;
using DuckyWorld.Object;

namespace DuckyWorld.Factory
{
    /// <summary>
    /// 오브젝트 생성 팩토리
    /// ObjectType별로 올바른 Logic 객체를 생성
    /// GC 최소화: new는 PoolManager가 풀에서 꺼낼 때만 수행
    /// </summary>
    public static class ObjectFactory
    {
        /// <summary>
        /// ObjectType에 따라 해당 타입의 Logic 객체 생성
        /// 풀에서 객체가 없을 때만 호출됨 (PoolManager.GetObject에서)
        /// </summary>
        public static T Create<T>(ObjectType type) where T : ObjectBase, new()
        {
            // 새 인스턴스 생성 (PoolManager가 풀 부족 시만 호출)
            var obj = new T();

            // 타입별 초기화
            InitializeByType(obj, type);

            return obj;
        }

        /// <summary>
        /// 타입별 Logic 초기화 로직
        /// 풀에서 꺼낼 때마다 OnSpawn에서 이 값들을 재설정하지 않도록 주의
        /// </summary>
        private static void InitializeByType(ObjectBase obj, ObjectType type)
        {
            obj.poolKey = type;

            switch (type)
            {
                // === 캐릭터 ===
                case ObjectType.Character_Duck_Default:
                    if (obj is CharacterBase charDefault)
                    {
                        charDefault.objectName = "Character_Duck_Default";
                        charDefault.hp = 100;
                        charDefault.maxHp = 100;
                        charDefault.moveSpeed = 5f;
                        charDefault.jumpForce = 10f;
                        charDefault.attackPower = 10f;
                        charDefault.collisionLayer = CollisionLayer.Character;
                        charDefault.collisionMask = CollisionLayer.Obstacle | CollisionLayer.Monster;
                    }
                    break;

                case ObjectType.Character_Duck_Ski:
                    if (obj is CharacterBase charSki)
                    {
                        charSki.objectName = "Character_Duck_Ski";
                        charSki.hp = 120;
                        charSki.maxHp = 120;
                        charSki.moveSpeed = 7f;
                        charSki.jumpForce = 12f;
                        charSki.attackPower = 8f;
                        charSki.collisionLayer = CollisionLayer.Character;
                        charSki.collisionMask = CollisionLayer.Obstacle | CollisionLayer.Monster;
                    }
                    break;

                // === 몬스터 ===
                case ObjectType.Monster_VillainDuck:
                    if (obj is MonsterBase monsterVillain)
                    {
                        monsterVillain.objectName = "Monster_VillainDuck";
                        monsterVillain.hp = 50;
                        monsterVillain.maxHp = 50;
                        monsterVillain.moveSpeed = 3f;
                        monsterVillain.attackPower = 5f;
                        monsterVillain.collisionLayer = CollisionLayer.Monster;
                        monsterVillain.collisionMask = CollisionLayer.Character | CollisionLayer.Projectile;
                        monsterVillain.detectionRange = 8f;
                        monsterVillain.attackRange = 2f;
                    }
                    break;

                // === 발사체 ===
                case ObjectType.Projectile_Snowball:
                    if (obj is PassiveObject projectile)
                    {
                        projectile.objectName = "Projectile_Snowball";
                        projectile.hp = 1;
                        projectile.maxHp = 1;
                        projectile.velocity = 15f;
                        projectile.collisionLayer = CollisionLayer.Projectile;
                        projectile.collisionMask = CollisionLayer.Monster | CollisionLayer.Obstacle;
                    }
                    break;

                // === 이펙트 ===
                case ObjectType.Effect_Explosion:
                    if (obj is DrawOnlyObject effectExplosion)
                    {
                        effectExplosion.objectName = "Effect_Explosion";
                        effectExplosion.duration = 0.5f;
                        effectExplosion.viewId = -1;  // View는 ViewPool에서 생성
                    }
                    break;

                case ObjectType.Effect_ScorePopup:
                    if (obj is DrawOnlyObject effectPopup)
                    {
                        effectPopup.objectName = "Effect_ScorePopup";
                        effectPopup.duration = 1.5f;
                        effectPopup.viewId = -1;
                    }
                    break;

                // === 장애물 (아직 ObstacleBase 클래스 미구현) ===
                // 추후 ObstacleBase 클래스 완성 후 추가
                case ObjectType.Obstacle_Tree:
                case ObjectType.Obstacle_Snowman:
                case ObjectType.Obstacle_Balloon:
                    Debug.LogWarning($"[ObjectFactory] ObstacleBase 클래스 미구현: {type}");
                    break;

                default:
                    Debug.LogWarning($"[ObjectFactory] 알 수 없는 ObjectType: {type}");
                    break;
            }
        }
    }
}
