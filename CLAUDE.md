# DuckyWorld - Claude Instructions

## 프로젝트 개요
- **게임명**: DuckyWorld
- **장르**: 캐주얼 미니게임 컬렉션 (길건너친구들 스타일 비주얼 + 미니게임천국 스타일 게임)
- **메인 캐릭터**: 오리 (로우폴리 3D 모델링)
- **플랫폼**: iOS / Android / PC
- **엔진**: Unity (URP)
- **백엔드**: Firebase / Photon PUN2

## 폴더 구조
```
ClaudeTest/              ← Unity 게임 프로젝트 루트
├── CLAUDE.md            ← Claude Code 지시 파일 (루트 고정)
└── .claude/             ← Claude 관련 파일 전용 폴더
    ├── skills/          ← 프로젝트 전용 스킬
    │   └── minigame-design/
    ├── docs/            ← 기획/설계 문서
    │   ├── tech_architecture.md  ← 기술 설계 문서
    │   ├── economy_system.md     ← 경제 / 캐릭터 / 장비 시스템
    │   └── design_workflow.md    ← 디자인 논의 서브에이전트 운용 방식
    ├── minigames/       ← 미니게임 기획서
    │   ├── _template.md ← 미니게임 기획서 템플릿
    │   └── XX_game.md   ← 미니게임 기획서들 (번호_이름 형식)
    ├── DuckyWorld_GDD.md ← 전체 게임 기획서 (메인 문서)
    └── temp/            ← 메모리 참조용
```

## 작업 규칙
- 사용자와의 대화는 항상 **한글**로
- 기획 내용이 추가/변경되면 관련 `.claude/` 내 md 파일을 **즉시 업데이트**
- 새 미니게임 기획 시 `.claude/minigames/_template.md` 기반으로 작성
- 새 미니게임 추가 시 `.claude/DuckyWorld_GDD.md` 미니게임 목록만 업데이트
- 파일명은 **영문**으로만 작성


## 현재 진행 상황
- 전체 기획서: 작성 중 (기획 단계)
- 기술 설계 문서: 초안 작성 완료
- 개발 미착수
