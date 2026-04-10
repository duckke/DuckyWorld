using DuckyWorld.Object;

namespace DuckyWorld.Input
{
    /// <summary>
    /// 탭 입력 핸들러
    /// 단순 탭(짧은 터치)만 수신하는 미니게임 입력 선언
    /// 예: 점프, 버튼 누르기
    /// </summary>
    public class TapInput : IGameInput
    {
        private static readonly InputType[] s_supported = { InputType.Tap };
        public InputType[] SupportedInputTypes => s_supported;

        private ActiveObject _target;

        public TapInput(ActiveObject target)
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
