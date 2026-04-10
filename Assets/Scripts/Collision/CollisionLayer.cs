namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌 레이어 (비트 마스크)
    /// </summary>
    [System.Flags]
    public enum CollisionLayer
    {
        None       = 0,
        Character  = 1 << 0,  // 1
        Monster    = 1 << 1,  // 2
        Obstacle   = 1 << 2,  // 4
        Projectile = 1 << 3,  // 8
        Item       = 1 << 4,  // 16
    }
}
