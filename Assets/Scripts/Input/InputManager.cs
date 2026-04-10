using System.Collections.Generic;
using UnityEngine;
using DuckyWorld.Core;

namespace DuckyWorld.Input
{
    /// <summary>
    /// 입력 관리자 (MonoSingleton)
    /// - 터치 → Enqueue → preProc에서 ProcessQueue → IGameInput 핸들러 호출
    /// - 스와이프 판정: fingerId별 시작 위치 추적, TouchUp 시 delta로 판정
    /// - 연타 판정: 시간 윈도우 내 탭 횟수 카운트
    /// - PvP 대비: InputData.frameNumber 포함 (추후 InputSync 연동)
    /// </summary>
    public class InputManager : MonoSingleton<InputManager>
    {
        // === 설정 ===
        [SerializeField] private float swipeThreshold = 50f;        // 스와이프 최소 거리 (px)
        [SerializeField] private float tapMaxDistance = 20f;        // 탭 최대 이동 거리 (px)
        [SerializeField] private float rapidTapWindow = 0.5f;       // 연타 시간 윈도우 (초)
        [SerializeField] private int rapidTapMinCount = 3;          // 연타 최소 탭 횟수

        // === 입력 큐 (GC 최소화: 고정 크기 링 버퍼 대신 List 사용, 프레임마다 Clear) ===
        private readonly List<InputData> _inputQueue = new List<InputData>(32);

        // === 핸들러 등록 목록 ===
        private readonly List<IGameInput> _handlers = new List<IGameInput>(8);

        // === 스와이프 추적 (fingerId → TouchDown 위치) ===
        private readonly Dictionary<int, Vector2> _touchStartPos = new Dictionary<int, Vector2>(5);

        // === 연타 추적 ===
        private readonly List<float> _tapTimestamps = new List<float>(16);

        // === PvP용 프레임 번호 ===
        private int _frameNumber = 0;

        // -----------------------------------------------------------------------
        // 핸들러 등록/해제
        // -----------------------------------------------------------------------

        /// <summary>
        /// IGameInput 핸들러 등록 (미니게임 시작 시 호출)
        /// </summary>
        public void RegisterHandler(IGameInput handler)
        {
            if (handler == null) return;
            if (!_handlers.Contains(handler))
                _handlers.Add(handler);
        }

        /// <summary>
        /// IGameInput 핸들러 해제 (미니게임 종료 시 호출)
        /// </summary>
        public void UnregisterHandler(IGameInput handler)
        {
            if (handler == null) return;
            _handlers.Remove(handler);
        }

        /// <summary>
        /// 모든 핸들러 해제
        /// </summary>
        public void ClearHandlers()
        {
            _handlers.Clear();
        }

        // -----------------------------------------------------------------------
        // Unity 입력 수집 (매 Update에서 호출, Logic 체인과 분리)
        // -----------------------------------------------------------------------

        private void Update()
        {
            CollectTouches();
        }

        /// <summary>
        /// 터치 입력 수집 → 입력 큐에 원시 이벤트 Enqueue
        /// 스와이프/연타 판정은 ProcessQueue에서 수행
        /// </summary>
        private void CollectTouches()
        {
#if UNITY_EDITOR
            // 에디터: 마우스 입력으로 시뮬레이션 (fingerId = 0)
            CollectMouseInput();
#else
            int touchCount = Input.touchCount;
            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                InputType type = touch.phase switch
                {
                    TouchPhase.Began   => InputType.TouchDown,
                    TouchPhase.Moved   => InputType.TouchMove,
                    TouchPhase.Stationary => InputType.TouchMove,
                    TouchPhase.Ended   => InputType.TouchUp,
                    TouchPhase.Canceled => InputType.TouchUp,
                    _ => InputType.TouchMove
                };

                Enqueue(new InputData
                {
                    inputType    = type,
                    fingerId     = touch.fingerId,
                    position     = touch.position,
                    frameNumber  = _frameNumber,
                });
            }
#endif
        }

#if UNITY_EDITOR
        private void CollectMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Enqueue(new InputData
                {
                    inputType   = InputType.TouchDown,
                    fingerId    = 0,
                    position    = UnityEngine.Input.mousePosition,
                    frameNumber = _frameNumber,
                });
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                Enqueue(new InputData
                {
                    inputType   = InputType.TouchMove,
                    fingerId    = 0,
                    position    = UnityEngine.Input.mousePosition,
                    frameNumber = _frameNumber,
                });
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                Enqueue(new InputData
                {
                    inputType   = InputType.TouchUp,
                    fingerId    = 0,
                    position    = UnityEngine.Input.mousePosition,
                    frameNumber = _frameNumber,
                });
            }
        }
#endif

        // -----------------------------------------------------------------------
        // 외부 Enqueue (PvP: InputSync에서 서버 수신 입력 주입)
        // -----------------------------------------------------------------------

        /// <summary>
        /// 원시 입력 이벤트를 큐에 추가
        /// 싱글: CollectTouches()에서 자동 호출
        /// PvP: InputSync가 서버 동기화 후 호출
        /// </summary>
        public void Enqueue(InputData data)
        {
            _inputQueue.Add(data);
        }

        // -----------------------------------------------------------------------
        // ProcessQueue — GameManager의 preProc 단계에서 호출
        // -----------------------------------------------------------------------

        /// <summary>
        /// 큐에 쌓인 입력을 처리하여 IGameInput 핸들러로 전달
        /// 스와이프/연타 판정 포함
        /// GameManager.DoUpdate() → preProc 단계에서 호출
        /// </summary>
        public void ProcessQueue()
        {
            _frameNumber++;

            int count = _inputQueue.Count;
            for (int i = 0; i < count; i++)
            {
                InputData raw = _inputQueue[i];
                ProcessRawInput(raw);
            }
            _inputQueue.Clear();

            // 연타 타임스탬프 중 윈도우 밖은 정리
            PruneRapidTapTimestamps();
        }

        /// <summary>
        /// 원시 입력 1개를 분석 → 고수준 InputType(Tap/Swipe/RapidTap)으로 변환 후 발행
        /// </summary>
        private void ProcessRawInput(InputData raw)
        {
            switch (raw.inputType)
            {
                case InputType.TouchDown:
                    HandleTouchDown(raw);
                    break;

                case InputType.TouchMove:
                    // 이동 자체는 핸들러에 그대로 전달 (조이스틱 등에서 사용 가능)
                    DispatchToHandlers(raw);
                    break;

                case InputType.TouchUp:
                    HandleTouchUp(raw);
                    break;

                case InputType.JoystickMove:
                    DispatchToHandlers(raw);
                    break;
            }
        }

        private void HandleTouchDown(InputData raw)
        {
            // 시작 위치 기록 (스와이프 판정용)
            _touchStartPos[raw.fingerId] = raw.position;

            // TouchDown 자체도 핸들러에 전달 (필요한 경우)
            DispatchToHandlers(raw);
        }

        private void HandleTouchUp(InputData raw)
        {
            // 시작 위치가 없으면 무시 (TouchDown 없이 Up이 온 경우)
            if (!_touchStartPos.TryGetValue(raw.fingerId, out Vector2 startPos))
            {
                DispatchToHandlers(raw);
                return;
            }

            _touchStartPos.Remove(raw.fingerId);
            Vector2 delta = raw.position - startPos;
            float distance = delta.magnitude;

            if (distance > swipeThreshold)
            {
                // 스와이프 판정
                InputData swipeData = raw;
                swipeData.inputType = InputType.Swipe;
                swipeData.delta = delta;
                DispatchToHandlers(swipeData);
            }
            else if (distance <= tapMaxDistance)
            {
                // 탭 판정
                InputData tapData = raw;
                tapData.inputType = InputType.Tap;

                // 연타 카운트 업데이트
                float now = Time.time;
                _tapTimestamps.Add(now);
                PruneRapidTapTimestamps();

                if (_tapTimestamps.Count >= rapidTapMinCount)
                {
                    // 연타 판정
                    InputData rapidData = tapData;
                    rapidData.inputType = InputType.RapidTap;
                    rapidData.tapCount  = _tapTimestamps.Count;
                    DispatchToHandlers(rapidData);
                }
                else
                {
                    DispatchToHandlers(tapData);
                }
            }
            else
            {
                // 범위 사이 → 일반 TouchUp 전달
                DispatchToHandlers(raw);
            }
        }

        /// <summary>
        /// 연타 시간 윈도우 밖의 타임스탬프 제거
        /// </summary>
        private void PruneRapidTapTimestamps()
        {
            float cutoff = Time.time - rapidTapWindow;
            // 앞부터 오래된 것 제거
            int removeCount = 0;
            for (int i = 0; i < _tapTimestamps.Count; i++)
            {
                if (_tapTimestamps[i] < cutoff) removeCount++;
                else break;
            }
            if (removeCount > 0)
                _tapTimestamps.RemoveRange(0, removeCount);
        }

        /// <summary>
        /// InputData를 지원하는 핸들러에만 전달
        /// </summary>
        private void DispatchToHandlers(InputData data)
        {
            int handlerCount = _handlers.Count;
            for (int h = 0; h < handlerCount; h++)
            {
                IGameInput handler = _handlers[h];
                InputType[] supported = handler.SupportedInputTypes;
                for (int s = 0; s < supported.Length; s++)
                {
                    if (supported[s] == data.inputType)
                    {
                        handler.OnInput(data);
                        break;
                    }
                }
            }
        }

        // -----------------------------------------------------------------------
        // 조이스틱 입력 (UI에서 직접 호출)
        // -----------------------------------------------------------------------

        /// <summary>
        /// 가상 조이스틱 UI에서 호출 — 조이스틱 입력을 큐에 추가
        /// </summary>
        public void EnqueueJoystick(Vector2 value)
        {
            Enqueue(new InputData
            {
                inputType     = InputType.JoystickMove,
                fingerId      = -1,
                joystickValue = value,
                frameNumber   = _frameNumber,
            });
        }

        // -----------------------------------------------------------------------
        // 디버그
        // -----------------------------------------------------------------------

        public int QueueCount    => _inputQueue.Count;
        public int HandlerCount  => _handlers.Count;
        public int FrameNumber   => _frameNumber;
    }
}
