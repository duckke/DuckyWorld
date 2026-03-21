# DuckyWorld - 게임 기획서

> **DuckyWorld**는 로우폴리 오리 캐릭터를 조종하는 캐주얼 미니게임 컬렉션입니다.
> 각 게임은 1분 내외의 짧은 플레이 시간으로 설계되어 있으며, iOS / Android / PC를 지원합니다.

---

## 기획서 구조

```
duckyworld/
├── concept.md                    : 게임 컨셉, 캐릭터, 세계관
├── art_style.md                  : 아트 방향, UI 스타일, 오디오
├── monetization.md               : 수익화 모델
├── gameplay/
│   ├── ui_flow.md                : UI 흐름 (로비~게임오버)
│   ├── account.md                : 계정 시스템
│   ├── pvp.md                    : PvP 대전 (매칭, 화면분할, 능력치)
│   ├── ranking_season.md         : 싱글 랭킹, 시즌제
│   ├── tier.md                   : 티어 시스템
│   ├── friends.md                : 친구 시스템
│   └── tutorial.md               : 튜토리얼
├── systems/
│   ├── economy.md                : 재화(골드), 골드 순환
│   ├── character.md              : 오리 획득, 스태미나, 스탯
│   └── equipment.md              : 장비, 능력치, 강화
├── technology/
│   ├── architecture.md           : 엔진(Unity URP), 기술 스택
│   ├── backend_firebase.md       : Firebase Auth, Firestore
│   ├── networking_photon.md      : Photon PUN2, 입력 동기화
│   ├── scene_structure.md        : Unity 씬 구조
│   ├── data_storage.md           : 데이터 저장 전략
│   ├── localization.md           : 로컬라이제이션
│   └── ads_iap.md                : 광고, 인앱 결제
└── minigames/
    ├── _template.md              : 미니게임 기획서 템플릿
    ├── improvements.md           : 토론 기록 (개선점 + 신규 게임)
    ├── 01_thump_thump_slope.md   : 스키
    ├── 02_flap_flap.md           : 플래피덕
    ├── 03_waddle_sprint.md       : 달리기
    ├── 04_hoppy_forest.md        : 허들
    ├── 05_slide_run.md           : 슬라이드런
    ├── 06_narrow_path.md         : 좁은길
    └── 07_villain_dodge.md       : 악당피하기
```

