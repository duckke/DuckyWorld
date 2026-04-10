using UnityEngine;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌 없는 이펙트/연출용 오브젝트
    /// duration 만료 시 자동 회수 요청
    /// </summary>
    public class DrawOnly : ObjectBase
    {
        // 지속 시간 (초과 시 자동 회수)
        public float duration { get; set; } = 0f;

        public DrawOnly()
        {
            poolKey = ObjectType.Effect;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            objectTimer = new DWTimer(duration);
        }

        public override void mainProc(float dt)
        {
            base.mainProc(dt);

            // duration 초과 시 자동 회수 예약
            if (duration > 0 && objectTimer.IsFinished())
            {
                OnDespawn();
                // PoolManager.ReturnObject(this) 호출은 ModuleBase 레벨에서 처리
            }
        }
    }
}
