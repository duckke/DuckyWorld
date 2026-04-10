namespace DuckyWorld.Input
{
    /// <summary>
    /// 미니게임이 사용하는 입력 타입을 선언하는 인터페이스
    /// 실제 판정 로직은 InputManager에 집중, 이 구현체는 "어떤 입력을 쓸지" 선언만 함
    /// </summary>
    public interface IGameInput
    {
        /// <summary>
        /// 이 입력 핸들러가 처리할 InputType 목록
        /// InputManager.ProcessQueue()는 이 타입만 해당 오브젝트에 전달
        /// </summary>
        InputType[] SupportedInputTypes { get; }

        /// <summary>
        /// 처리된 InputData를 수신하여 오브젝트 상태에 반영
        /// </summary>
        void OnInput(InputData data);
    }
}
