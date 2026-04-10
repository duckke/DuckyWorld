namespace DuckyWorld.Factory
{
    /// <summary>
    /// 오브젝트 풀 키 enum
    /// ObjectFactory에서 타입별 생성, PoolManager에서 풀 관리
    /// </summary>
    public enum ObjectType
    {
        // 캐릭터
        Character_Duck_Default,
        Character_Duck_Ski,

        // 몬스터
        Monster_VillainDuck,

        // 장애물
        Obstacle_Tree,
        Obstacle_Snowman,
        Obstacle_Balloon,

        // 발사체
        Projectile_Snowball,

        // 이펙트
        Effect_Explosion,
        Effect_ScorePopup,
    }
}
