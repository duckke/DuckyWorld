using DuckyWorld.Object;

namespace DuckyWorld.Input
{
    /// <summary>
    /// 연타 입력 핸들러
    /// 시간 윈도우 내 연속 탭을 수신하는 미니게임 입력 선언
    /// 실제 연타 판정(시간 윈도우 내 N회)은 InputManager에서 처리
    /// 예: 달리기 가속, 연속 공격
    /// </summary>
    public class RapidTapInput : IGameInput
    {
        private static readonly InputType[] s_supported = { InputType.RapidTap };
        public InputType[] SupportedInputTypes => s_supported;

        private ActiveObject _target;

        public RapidTapInput(ActiveObject target)
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
