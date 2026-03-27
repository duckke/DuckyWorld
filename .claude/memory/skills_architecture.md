---
name: DuckyWorld 스킬/에이전트 구조
description: 스킬은 단일 SKILL.md 진입점, 에이전트는 역할 정의 — 라우터/서브스킬 구조 폐지
type: project
---

# DuckyWorld 스킬/에이전트 구조

## 핵심 원칙

- **Skill = 사용자 요청의 진입점** (단일 SKILL.md)
- **Agent = 역할 정의** (여러 스킬에서 재사용 가능)
- 불필요한 라우터/서브스킬 구조 폐지 → 단순하고 명확하게

## 현재 구조

### skills/
```
skills/
├── git-commit/SKILL.md       ← 커밋 메시지 형식 + 워크플로우
├── minigame-design/SKILL.md  ← 크리/로직 토론 기반 기획
├── claude-env/SKILL.md       ← Claude 환경 검토/개선 → 깔끔이 위임
└── notion-sync/SKILL.md      ← 기획서 md를 Notion에 동기화 → notion-editor 위임
```

### agents/
```
agents/
├── design/                   ← 기획 팀
│   ├── leader.md             (꼼꼼이 - 기획 팀장)
│   ├── team-rules.md         (기획팀 규칙)
│   └── members/
│       ├── explorer.md       (뒤적이 - 읽기 전용 탐색)
│       └── notion-editor.md  (Notion 포매터)
├── develop/                  ← 개발 팀
│   ├── leader.md             (뚝딱이 - 개발 팀장)
│   ├── team-rules.md         (개발팀 규칙)
│   └── members/
│       ├── explorer.md       (뒤적이 - 읽기 전용 탐색)
│       └── unity-editor.md   (유니티 - Unity Editor MCP 조작)
└── pm/                       ← PM 팀
    ├── leader.md             (깔끔이 - PM 팀장)
    ├── team-rules.md         (PM팀 규칙)
    └── members/
        └── explorer.md       (뒤적이 - 읽기 전용 탐색)
```
