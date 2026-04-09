# 엔진(Unity URP), 기술 스택

## 엔진 / 환경

| 항목 | 내용 |
|------|------|
| 엔진 | Unity 6000.4.0f1 |
| 렌더 파이프라인 | URP (Universal Render Pipeline) |
| 입력 시스템 | New Input System 활성화 |
| 타겟 플랫폼 | iOS / Android / PC |

## 핵심 설계 원칙

| 원칙 | 설명 |
|------|------|
| mainProc 체인 | pre → main → post 3단계 프레임 루프 |
| Logic-View 완전 분리 | Logic은 순수 C#, View는 GameObject (ViewObjectManager.SyncAll) |
| Factory + Pool | ObjectFactory/AppendageFactory + PoolManager (Object/Appendage/View 통합) |
| 모듈 기반 흐름 | ModuleBase → IntroModule / LobbyModule / MiniGameModule |
| GC 제로 충돌 시스템 | Burst Job 기반, NativeArray<LogicColliderData>, struct만 사용 |

## 프레임 루프 (GameManager)

- **고정 프레임 타임**: `FRAME_TIME = 0.0166667f` (60fps)
- `Time.deltaTime` 누적 → `FRAME_TIME` 초과 시마다 한 프레임 처리
- `gameTimer`는 FRAME_TIME 단위로만 증가 → 결정론적 (PvP 동기화 안전)

```
DoUpdate(dt):
  [Logic Proc]
  1. pre  — SpawnQueue, DespawnQueue, InputManager.ProcessQueue, Module.preProc
  2. main — Module.mainProc (오브젝트 이동/상태 + CollisionManager.CheckCollisions)
  3. post — Module.postProc (CollisionManager.ProcessResults, 생성/삭제 예약)

  [Non-Logic Proc]
  4. ViewObjectManager.SyncAll (position/rotation/anim/facing/visible 비교 갱신)
  5. UIManager.Proc
```

## 오브젝트 계층

```
ObjectBase (Logic 전용, 하이어라키 X, 순수 C#)
├── DrawOnly (이펙트/연출, 충돌 없음)
└── CollisionObject (HP, LogicCollider, CollisionLayer)
    ├── ObstacleBase (장애물)
    ├── PassiveObject (자체이동, 조작X)
    └── ActiveObject (상태머신, Appendage, Equipment)
        ├── CharacterBase (CharacterInfo)
        └── MonsterBase (AI)
```

## 입력 시스템

- **InputData** struct: `InputType` (TouchDown/TouchMove/TouchUp/JoystickMove), `fingerId`, `position`, `joystickValue`, `frameNumber`
- IGameInput 구현체: TapInput, SwipeInput, RapidTapInput, JoystickInput — 미니게임별 입력 타입 선언
- 스와이프/연타 판정 로직은 InputManager에 집중

## 풀링 (PoolManager)

- **GameManager.Pool** 로 통합 접근
- Object 풀 (Dictionary<ObjectType, Queue<ObjectBase>>)
- Appendage 풀 (Dictionary<AppendageType, Queue<Appendage>>)
- View 풀 (Dictionary<ObjectType, Queue<ObjectView>>)
- PreWarm으로 로딩 시 충분한 풀 사전 생성

## UI 시스템

- 단일 Canvas, 스택 기반 Push/Pop/Replace
- UIBase → Screens/ (UILobby, UIThumpThumpSlope 등) + Popup/ (UIPopupGameOver 등)
- UILoading: 씬 전환 전용 프로그레스바 (GameSceneLoader가 직접 참조)
- HUD 폴더 없음 — 각 씬의 최하단 UI를 HUD로 사용

## 명명 규칙

- 네임스페이스 없음, DW 접두사 없음 (DWTimer만 예외)
- **Info**: 코드에서 가공/조합된 런타임 정보 (CharacterInfo, EquipmentInfo 등)
- **Data**: 데이터 테이블에서 읽어들인 원본 데이터 (CharacterData, EquipmentData 등)

## 구현 단계

| Phase | 내용 | 비고 |
|-------|------|------|
| 1 | Core 뼈대 (GameManager, ModuleBase, PoolManager 등) | |
| 2 | Object 계층 (ObjectBase ~ MonsterBase) | |
| 3 | 충돌 시스템 (Burst Job, NativeArray) | |
| 4 | Factory (ObjectFactory, AppendageFactory) | |
| 5 | View 시스템 (ViewObjectManager, ObjectView) | |
| 6 | 입력 시스템 (InputManager, IGameInput) | |
| 7 | 미니게임 프레임워크 (MiniGameBase, MiniGameModule) | |
| 8 | UI 프레임워크 (UIManager, UILoading) | |
| 9 | Player + Data/Info + Bundle + 씬 전환 | |
| 10 | 에디터 툴 (ColliderExtractor) | |
| 11 | Module 구현 (Intro, Lobby) | |
| 12 | 첫 미니게임 (퍼덕퍼덕) | 전체 아키텍처 검증 |
| 13 | 백엔드 + 네트워크 | 후순위 — 싱글 우선 |
| 14 | 경제 시스템 | 후순위 |

> 상세 코드 구조: [code_architecture_v4.md](../code_architecture_v4.md) 참고
