# UI 흐름 (로비~게임오버)

## 화면 흐름

```
Intro (Firebase 초기화, 로그인, 번들 로드)
  ↓  GameSceneLoader + UILoading (프로그레스바)
Lobby (로비 - 캐릭터 뛰어노는 3D 씬)
  ↓
GameSelect (게임 선택 — Lobby 위 UIManager 스택 push)
  ↓  GameSceneLoader + UILoading (프로그레스바)
InGame (미니게임 플레이)
  ├── Ready → 카운트다운 UI
  ├── Playing → 게임 HUD
  ├── Paused → 일시정지 팝업
  └── End → GameOver 결과 화면 (스코어, 랭킹 갱신 여부)
  ↓
Lobby 또는 GameSelect로 복귀
```

## 씬 전환

- **GameSceneLoader**: MonoSingleton, `LoadScene(sceneName, nextModule)` 호출
- **UILoading**: 전체 화면을 덮는 로딩 UI (프로그레스바 + 팁 텍스트)
- UIManager 스택과 별도로 GameSceneLoader가 직접 참조

## UI 시스템

- **단일 Canvas**, 스택 기반 Push/Pop/Replace (UIManager)
- HUD 폴더 없음 — 각 씬의 최하단 UI를 HUD로 사용 (닫지 못하게)
- UI 갱신은 GameManager의 Non-Logic Proc (ViewSync 이후)에서 `UIManager.Proc(dt)` 호출

## 로비

- 캐릭터가 뛰어노는 3D 씬
- 메인 캐릭터 선택 가능
- UI 컬러 테마 상세: [art/art_style.md](../art/art_style.md) 참고

## 게임 선택

- Lobby 위에 UIManager 스택으로 push

> 추후 상세화 예정

## 미니게임 상태 전이 (UI 연동)

```
None → Ready   : MiniGameModule.OnEnter() 내부
Ready → Playing : UI 카운트다운 완료 후 UI가 MiniGameModule.StartGame() 호출
Playing → Paused : UI 일시정지 버튼 → MiniGameModule.PauseGame()
Paused → Playing : UI 재개 버튼 → MiniGameModule.ResumeGame()
Playing → End    : 게임 내부 조건 충족 → MiniGameModule.EndGame()
End → Ready      : UI 재시작 버튼 → MiniGameModule.RestartGame()
```
