# Claude Instructions 구조 개선 가이드

## 개요

CLAUDE.md를 역할/도메인별로 분리하여 각 에이전트가 필요한 정보만 효율적으로 로드할 수 있도록 개선했습니다.

## 디렉토리 구조

```
.claude/
├── STRUCTURE.md              ← 이 문서
├── agents/
│   ├── develop/              ← 개발팀
│   │   ├── CLAUDE.md         ← 상세 규칙 (rules/develop에서 복사)
│   │   ├── leader.md         ← 뚝딱이 (팀장)
│   │   └── members/
│   │       └── explorer.md   ← 뒤적이 (탐색 담당)
│   ├── design/               ← 기획팀
│   │   ├── CLAUDE.md         ← 상세 규칙 (rules/design에서 복사)
│   │   ├── leader.md         ← 촘촘이 (팀장)
│   │   └── members/
│   │       ├── explorer.md   ← 뒤적이 (탐색 담당)
│   │       └── notion-editor.md ← 다듬이 (포맷팅 담당)
│   ├── pm/                   ← PM/구조팀
│   │   ├── CLAUDE.md         ← 상세 규칙 (rules/pm에서 복사)
│   │   ├── leader.md         ← 말끔이 (팀장)
│   │   └── members/
│   │       └── explorer.md   ← 뒤적이 (탐색 담당)
│   └── common/
│       └── explorer.md       ← 뒤적이 (레거시 위치 유지)
├── rules/                    ← 역할별 상세 규칙 (원본)
│   ├── develop/
│   │   └── CLAUDE.md         ← 개발 관련 모든 규칙
│   ├── design/
│   │   └── CLAUDE.md         ← 기획 관련 모든 규칙
│   └── pm/
│       └── CLAUDE.md         ← PM/구조 관련 모든 규칙
├── docs/
│   ├── duckyworld.md
├── skills/
│   ├── git-commit/
│   ├── minigame-design/
│   └── notion-sync/
└── memory/
    └── MEMORY.md
```

## 파일 역할

### 메인 컨텍스트용: `/CLAUDE.md`
- 조직 구조 (역할, 팀 목록)
- 비서 행동 원칙 (라우팅만)
- 글로벌 규칙 (언어, 파일명)
- 각 팀의 leader.md 위치 참조

### 팀장 파일: `.claude/agents/[role]/leader.md`
- 팀 소개 (한 문장)
- 상세 규칙 참조 (CLAUDE.md)
- 팀 구조 (팀원 목록)
- 주요 책임
- 워크플로우 (간략)

### 역할별 상세 규칙: `.claude/agents/[role]/CLAUDE.md`
- 사고 방식 / 핵심 가치
- 프로젝트 컨텍스트 (엔진, 언어, 플랫폼 등)
- 코딩·기획·구조 원칙
- 리뷰 체크리스트
- 상세 워크플로우 (STEP 1~5)
- 반환 형식
- 작업 규칙 (해당 역할만)

### 팀 구성원: `.claude/agents/[role]/members/[member].md`
- explorer.md: 탐색 전담 (각 팀별 복제)
- notion-editor.md: 기획팀만 (Notion 포맷팅)

## 각 팀이 로드할 파일

### 뚝딱이 (개발팀장)
```
1. /CLAUDE.md              (비서의 라우팅 정보)
2. .claude/agents/develop/CLAUDE.md   (상세 규칙)
```

### 촘촘이 (기획팀장)
```
1. /CLAUDE.md              (비서의 라우팅 정보)
2. .claude/agents/design/CLAUDE.md    (상세 규칙)
```

### 말끔이 (PM팀장)
```
1. /CLAUDE.md              (비서의 라우팅 정보)
2. .claude/agents/pm/CLAUDE.md        (상세 규칙)
```

## 재구조화 요약 (팀 구조 확립)

### 변경 사항
1. **팀 폴더 구조 확립**
   - `.claude/agents/develop/` → leader.md + CLAUDE.md + members/
   - `.claude/agents/design/` → leader.md + CLAUDE.md + members/
   - `.claude/agents/pm/` → leader.md + CLAUDE.md + members/

2. **팀장 파일 생성** (leader.md)
   - 각 팀의 오버뷰, 팀 구조, 주요 책임 포함
   - 상세 규칙은 CLAUDE.md에 분리

3. **CLAUDE.md 팀 폴더로 복사**
   - `.claude/rules/[role]/CLAUDE.md` → `.claude/agents/[role]/CLAUDE.md`
   - 원본은 rules/에 유지 (참조용)

4. **팀 구성원 배치**
   - explorer.md: 각 팀의 members/에 복제
   - notion-editor.md: design/members/로 이동
   - 기존 cleaner.md, designer.md, developer.md 삭제

5. **메인 CLAUDE.md 업데이트**
   - 라우팅 경로: leader.md로 변경
   - 규칙 참조: agents/[role]/CLAUDE.md로 변경

### 보존된 내용
- 모든 원칙, 워크플로우, 규칙 100% 보존
- 조직 구조, 라우팅 정보 동일
- 기획서 관련 규칙 (duckyworld.md) 포함

### 개선 효과
- 팀 구조 명확화: leader.md가 팀장 오버뷰 제공
- 팀원 관리: members/ 폴더로 팀 구성원 명시
- 경로 일관성: agents/ 내에서 모든 규칙 참조 가능
- 유지보수 용이: 팀 추가 시 leader.md + members/ 구조만 복제

## 사용 방법

### 팀 호출 시 (비서)
```
목표: [한 문장]
기대 산출물: [구체적 결과물]
제약: [있으면 명시]
```

각 팀장은 자동으로 자신의 CLAUDE.md를 로드합니다.
- 뚝딱이 → `.claude/agents/develop/CLAUDE.md` 참조
- 촘촘이 → `.claude/agents/design/CLAUDE.md` 참조
- 말끔이 → `.claude/agents/pm/CLAUDE.md` 참조

각 팀 내에서는 members/에 있는 팀원을 호출할 수 있습니다.
- 뒤적이 (탐색): `.claude/agents/[role]/members/explorer.md`
- 다듬이 (포맷팅): `.claude/agents/design/members/notion-editor.md`

## 향후 개선

### 스킬 규칙 분리 (옵션)
각 스킬 디렉토리 내 SKILL.md 구조:
```
.claude/skills/
├── git-commit/
│   ├── SKILL.md
│   └── router.md
├── minigame-design/
│   ├── SKILL.md
│   └── router.md
└── notion-sync/
    ├── SKILL.md
    └── router.md
```

### 문서 컨벤션 가이드 (옵션)
`.claude/agents/design/members/notion-editor.md` 현황:
- Notion 포맷팅 규칙 이미 구현됨
- 추가 확장은 필요에 따라

## 효과

- **컨텍스트 효율**: 각 에이전트가 자신의 규칙만 로드 → 토큰 절약
- **명확성**: 역할별 책임이 파일 구조에 명시적으로 드러남
- **유지보수**: 변경 시 해당 역할 파일만 수정
- **확장성**: 새 역할 추가 시 `.claude/rules/[new]/CLAUDE.md` 생성만 하면 됨
