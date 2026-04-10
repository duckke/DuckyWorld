using UnityEngine;

namespace DuckyWorld.Input
{
    /// <summary>
    /// 터치/입력 원시 타입
    /// </summary>
    public enum InputType
    {
        TouchDown,      // 터치 시작
        TouchMove,      // 터치 이동 (드래그)
        TouchUp,        // 터치 종료
        JoystickMove,   // 조이스틱 입력
        Tap,            // 단순 탭 (TouchDown + TouchUp, 이동 없음)
        Swipe,          // 스와이프 (TouchDown → TouchUp, delta > threshold)
        RapidTap,       // 연타 (시간 윈도우 내 탭 N회 이상)
    }

    /// <summary>
    /// 입력 데이터 (struct, GC 없음)
    /// PvP 대비: frameNumber 필드 포함
    /// </summary>
    public struct InputData
    {
        public InputType inputType;
        public int fingerId;            // 멀티터치 구분 (0, 1, 2...)
        public Vector2 position;        // 터치 좌표 (스크린)
        public Vector2 delta;           // 스와이프 방향+크기 (TouchUp 시 채워짐)
        public Vector2 joystickValue;   // 조이스틱 방향+크기 (-1~1)
        public int frameNumber;         // PvP 동기화용 (gameTimer 기반 프레임 번호)
        public int tapCount;            // RapidTap 판정 시 연타 횟수
    }
}
