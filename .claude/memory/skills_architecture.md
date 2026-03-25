---
name: DuckyWorld 스킬/에이전트 구조
description: 스킬은 단일 SKILL.md (라우터 폐지), 에이전트는 역할별 분리 (design + develop)
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
├── git-commit/SKILL.md      ← 커밋 메시지 형식 + 워크플로우
└── minigame-design/SKILL.md  ← 크리/로직 토론 기반 기획
```

### agents/
```
agents/
├── design/                   ← 기획/디자인 토론
│   ├── creative.md           (크리 - 감성/재미/독창성)
│   └── logic.md              (로직 - 밸런스/구현/일관성)
└── develop/                  ← 개발 작업
    ├── reviewer.md           (코드 리뷰)
    ├── refactor.md           (리팩토링)
    └── coder.md              (기능 구현)
```
