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
DuckyWorld/              ← Unity 게임 프로젝트 루트
├── CLAUDE.md            ← Claude Code 지시 파일 (루트 고정)
└── .claude/
    ├── skills/          ← 프로젝트 전용 스킬들
    │   ├── git-commit/
    │   ├── minigame-design/
    │   ├── notion-sync/ ← md ↔ Notion 동기화 (notion_map.json 포함)
    │   └── claude/      ← Claude Code 환경 설정 정본 및 동기화 스크립트
    ├── agents/          ← 독립적 에이전트들
    │   ├── design/      ← 크리(감성/재미) + 로직(밸런스/구현)
    │   └── develop/     ← 리뷰어, 리팩터, 코더
    └── docs/            ← 기획/설계 문서
        ├── duckyworld.md              ← 메인 기획서 (프로젝트 소개 + 문서 트리)
        └── duckyworld/
            ├── concept.md             ← 게임 컨셉, 캐릭터, 세계관
            ├── art_style.md           ← 아트 방향, UI 스타일, 오디오
            ├── monetization.md        ← 수익화 모델
            ├── gameplay/              ← UI 흐름, 계정, PvP, 랭킹, 티어, 친구, 튜토리얼
            ├── systems/               ← 재화, 캐릭터, 장비
            ├── technology/            ← 엔진, Firebase, Photon, 씬 구조, 데이터, 로컬라이제이션, 광고/IAP
            └── minigames/             ← 미니게임 기획서 + 토론 기록
```

## 메인 기획서
- **반드시 `.claude/docs/duckyworld.md`를 먼저 읽고 시작** — 전체 기획 구조와 문서 경로 파악
- 기획 관련 작업 시 duckyworld.md의 문서 트리를 참고하여 해당 md 파일을 읽고 수정

## 작업 규칙
- 사용자와의 대화는 항상 **한글**로
- 기획 내용이 추가/변경되면 관련 `.claude/docs/duckyworld/` 내 md 파일을 **즉시 업데이트**
- **md 파일 수정 후 "노션도 동기화할까요?" 라고 물어볼 것** — notion-sync 스킬 사용
- 새 미니게임 기획 시 `.claude/docs/duckyworld/minigames/_template.md` 기반으로 작성
- 새 미니게임 추가 시 `.claude/docs/duckyworld.md` 미니게임 목록 업데이트
- 파일명은 **영문**으로만 작성

## Claude Code 환경 동기화 규칙

**[AUTO-SYNC] 주입 시** — 로컬 설정이 변경됨
- 사용자에게 커밋 여부를 물어볼 것
- 동의 시: `/tmp/claude_pending_push.json` 내용을 `.claude/skills/claude/settings.json`에 저장 → 커밋 & 푸시 → `/tmp/claude_pending_push.json` 삭제
- 거부 시: `/tmp/claude_pending_push.json` 삭제

**[AUTO-APPLY] 주입 시** — 리포지토리 설정이 로컬과 다름 (git pull 후 등)
- 사용자에게 새로운 설정을 적용할지 물어볼 것
- 동의 시: `bash .claude/skills/claude/sync.sh` 실행
- 거부 시: 아무것도 하지 않음

- 다른 기기에서 세팅 맞출 때 → `git pull` 후 자동 감지됨 (직접 요청 불필요)

## 스킬/에이전트 구조 규칙
- **Skill = 사용자 요청의 진입점** / **Agent = 역할 정의 (재사용 가능)**
- 단일 작업: SKILL.md만
- 서브스킬이 3개 이상 & 각각 고유 로직 → 라우터 + subskills/ 구조
- SKILL.md description: **한 문장, 50자 이내**
- 에이전트는 역할별로 `agents/` 하위에 배치


## 현재 진행 상황
- 전체 기획서: 작성 중 (기획 단계)
- 기술 설계 문서: 초안 작성 완료
- 개발 미착수
