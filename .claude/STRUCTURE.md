# Claude Instructions 구조 개선 가이드

## 개요

CLAUDE.md를 역할/도메인별로 분리하여 각 에이전트가 필요한 정보만 효율적으로 로드할 수 있도록 개선했습니다.

## 디렉토리 구조

```
.claude/
├── STRUCTURE.md              ← 이 문서
├── agents/
│   ├── develop/
│   │   └── developer.md       ← 뚝딱이 (개발자) - 규칙은 .claude/rules/develop 참조
│   ├── design/
│   │   └── designer.md        ← 촘촘이 (기획자) - 규칙은 .claude/rules/design 참조
│   ├── pm/
│   │   └── cleaner.md         ← 말끔이 (PM) - 규칙은 .claude/rules/pm 참조
│   └── common/
│       └── explorer.md        ← 뒤적이 (탐색)
├── rules/                     ← 역할별 상세 규칙 (NEW)
│   ├── develop/
│   │   └── CLAUDE.md          ← 개발 관련 모든 규칙
│   ├── design/
│   │   └── CLAUDE.md          ← 기획 관련 모든 규칙
│   └── pm/
│       └── CLAUDE.md          ← PM/구조 관련 모든 규칙
├── docs/
│   ├── duckyworld.md
│   └── current_state.md
├── skills/
│   ├── git-commit/
│   ├── minigame-design/
│   └── notion-sync/
└── memory/
    └── MEMORY.md
```

## 파일 역할

### 메인 컨텍스트용: `/CLAUDE.md`
- 조직 구조 (역할, 에이전트 목록)
- 비서 행동 원칙 (라우팅만)
- 글로벌 규칙 (언어, 파일명)
- 각 역할 규칙 파일 위치 참조

### 에이전트 오버뷰: `.claude/agents/[role]/[name].md`
- 에이전트 소개 (한두 문장)
- 상세 규칙 참조 (`.claude/rules/[role]/CLAUDE.md`)
- 간단한 체크리스트 or 워크플로우 (선택)

### 역할별 상세 규칙: `.claude/rules/[role]/CLAUDE.md` ⭐ NEW
- 사고 방식 / 핵심 가치
- 프로젝트 컨텍스트 (엔진, 언어, 플랫폼 등)
- 코딩·기획·구조 원칙
- 리뷰 체크리스트
- 상세 워크플로우 (STEP 1~5)
- 반환 형식
- 작업 규칙 (해당 역할만)

## 각 에이전트가 로드할 파일

### 뚝딱이 (개발자)
```
1. /CLAUDE.md              (비서의 라우팅 정보)
2. .claude/rules/develop/CLAUDE.md   (상세 규칙)
```

### 촘촘이 (기획자)
```
1. /CLAUDE.md              (비서의 라우팅 정보)
2. .claude/rules/design/CLAUDE.md    (상세 규칙)
```

### 말끔이 (PM)
```
1. /CLAUDE.md              (비서의 라우팅 정보)
2. .claude/rules/pm/CLAUDE.md        (상세 규칙)
```

## 마이그레이션 요약

### 변경 사항
1. **`.claude/rules/` 디렉토리 생성** (3개 하위 디렉토리)
2. **역할별 CLAUDE.md 파일 생성** (develop, design, pm)
   - 기존 agent 파일의 상세 내용을 이동
   - 중복되는 전역 규칙은 제거
3. **메인 `/CLAUDE.md` 축약**
   - 라우팅 정보만 유지
   - 글로벌 규칙만 포함
   - 각 역할 규칙 파일 참조 추가
4. **에이전트 파일 헤더 업데이트**
   - `.claude/rules/[role]/CLAUDE.md` 참조 주석 추가

### 보존된 내용
- 모든 원칙, 워크플로우, 규칙 100% 보존
- 조직 구조, 라우팅 정보 동일
- 기획서 관련 규칙 (duckyworld.md, current_state.md) 포함

### 제거된 내용 (중복)
- 메인 CLAUDE.md에서 **상세 규칙만** 제거
  - 예: 코딩 원칙, 리뷰 체크리스트, 아이디어 평가 순서 등
  - 대신 역할별 파일에서 충분히 제공

## 사용 방법

### 에이전트 호출 시 (비서)
```
목표: [한 문장]
기대 산출물: [구체적 결과물]
제약: [있으면 명시]
```

에이전트는 자동으로 자신의 규칙 파일을 로드합니다.
- 뚝딱이 → `.claude/rules/develop/CLAUDE.md` 참조
- 촘촘이 → `.claude/rules/design/CLAUDE.md` 참조
- 말끔이 → `.claude/rules/pm/CLAUDE.md` 참조

## 향후 개선

### 스킬 규칙 분리 (옵션)
각 스킬 디렉토리 내 CLAUDE.md 추가:
```
.claude/skills/
├── git-commit/
│   └── CLAUDE.md
├── minigame-design/
│   └── CLAUDE.md
└── notion-sync/
    └── CLAUDE.md
```

### 문서 컨벤션 가이드 (옵션)
`.claude/rules/writing/CLAUDE.md` 추가:
- Notion 포맷팅 규칙
- md 파일 템플릿
- 기획서 구조

## 효과

- **컨텍스트 효율**: 각 에이전트가 자신의 규칙만 로드 → 토큰 절약
- **명확성**: 역할별 책임이 파일 구조에 명시적으로 드러남
- **유지보수**: 변경 시 해당 역할 파일만 수정
- **확장성**: 새 역할 추가 시 `.claude/rules/[new]/CLAUDE.md` 생성만 하면 됨
