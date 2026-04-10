using DuckyWorld.Object;

namespace DuckyWorld.Input
{
    /// <summary>
    /// 스와이프 입력 핸들러
    /// 방향 스와이프를 수신하는 미니게임 입력 선언
    /// 실제 스와이프 판정(delta > threshold)은 InputManager에서 처리
    /// 예: 슬라이드, 방향 공격
    /// </summary>
    public class SwipeInput : IGameInput
    {
        private static readonly InputType[] s_supported = { InputType.Swipe };
        public InputType[] SupportedInputTypes => s_supported;

        private ActiveObject _target;

        public SwipeInput(ActiveObject target)
        {
            _target = target;
        }

        public void OnInput(InputData data)
        {
            if (_target == null) return;
            _target.ApplyInput(data);
        }
    }
}
