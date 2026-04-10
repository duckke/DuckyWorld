using UnityEngine;

namespace DuckyWorld.Object
{
    /// <summary>
    /// 충돌 없는 이펙트/연출용 오브젝트
    /// duration 만료 시 자동 회수 요청
    /// poolKey는 ObjectFactory에서 구체적 ObjectType으로 설정됨
    /// </summary>
    public class DrawOnlyObject : ObjectBase
    {
        // 지속 시간 (초과 시 자동 회수)
        public float duration { get; set; } = 0f;

        public override void OnSpawn()
        {
            base.OnSpawn();
            // DWTimer는 struct이므로 Reset 후 duration 재설정
            objectTimer.Reset();
            objectTimer.duration = duration;
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
