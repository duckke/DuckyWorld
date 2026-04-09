# Unity 씬 구조

## 씬 목록

```
Scenes/
├── Intro.unity        — 인트로 (Firebase 초기화, 로그인, 번들 체크/다운로드/로드)
├── Lobby.unity        — 로비 (캐릭터 관리, 게임 선택)
└── Games/
    ├── ThumpThumpSlope.unity    — 두근두근 슬로프
    ├── FlutterFlutter.unity     — 퍼덕퍼덕
    ├── WaddleSprint.unity       — 뒤뚱뒤뚱 질주
    ├── HoppyForest.unity        — 폴짝폴짝 포레스트
    ├── SlideRun.unity           — 슬라이드런
    ├── NarrowPath.unity         — 아슬아슬 좁은길
    └── DodgeVillain.unity       — 피해피해 악당오리
```

## 씬-모듈 매핑

| 씬 | 모듈 | 역할 |
|----|------|------|
| Intro.unity | IntroModule | Firebase 초기화, 로그인, 번들 버전 체크 + 다운로드 + 로드 → DataRepository 등록 |
| Lobby.unity | LobbyModule | 캐릭터 관리, 게임 선택 |
| Games/*.unity | MiniGameModule | 미니게임 진행 (MiniGameFactory로 GameType별 생성) |

## 씬 전환 흐름 (GameSceneLoader)

```
1) UILoading.Show() — 현재 화면 가리기 (프로그레스바)
2) CurrentModule.OnExit() — 현재 모듈 정리
3) SceneManager.LoadSceneAsync(sceneName) — 비동기 씬 로딩 (0~50%)
4) nextModule.LoadResources(onProgress) — 모듈별 리소스 로드 (50~100%)
5) GameManager.SetModule(nextModule) → nextModule.OnEnter()
6) UILoading.Hide() — 다음 씬 노출
```

- **GameSceneLoader**: MonoSingleton, 어디서든 `LoadScene(sceneName, nextModule)` 호출
- **UILoading**: 전체 화면을 덮는 로딩 UI (UIManager 스택이 아닌 별도 관리), 프로그레스바 + 팁 텍스트
