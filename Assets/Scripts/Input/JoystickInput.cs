using DuckyWorld.Object;

namespace DuckyWorld.Input
{
    /// <summary>
    /// 조이스틱 입력 핸들러
    /// 가상 조이스틱의 연속 이동 입력을 수신하는 미니게임 입력 선언
    /// 예: 캐릭터 8방향 이동
    /// </summary>
    public class JoystickInput : IGameInput
    {
        private static readonly InputType[] s_supported = { InputType.JoystickMove };
        public InputType[] SupportedInputTypes => s_supported;

        private ActiveObject _target;

        public JoystickInput(ActiveObject target)
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
