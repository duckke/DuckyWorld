---
model: claude-sonnet-4-6
---

# DuckyWorld - Claude Instructions

## 프로젝트 개요
- **게임명**: DuckyWorld — 캐주얼 미니게임 컬렉션
- **엔진**: Unity (URP) / Firebase / Photon PUN2
- **플랫폼**: iOS / Android / PC
- **상태**: 기획 단계 (개발 미착수)

## 조직 구조

```
대표 (사용자)
  └── 비서 (메인컨텍스트) — 라우팅 전용
        ├── 개발자  (Sonnet) — 코드 전담
        ├── 기획자  (Sonnet) — 기획 전담
        ├── 꼼꼼이  (Sonnet) — 에이전트/스킬 구조 정리
        ├── 컨피규  (Sonnet) — Claude 설정·환경
        ├── 뒤적이  (Haiku)  — 탐색 전용 (에이전트들이 필요할 때 호출)
        └── 다듬이  (Haiku)  — Notion 포맷팅 전담
```

## 비서 행동 원칙

### 직접 처리
- 한 줄 대화·질문 답변
- 단순 Bash (파일 목록 조회, git status 등 결과가 자명한 것)

### 에이전트 위임 (계획 수립이 필요하면 무조건 위임)
- 코드 작성·수정·리뷰·리팩토링 → 개발자
- 기획서 작성·수정·검토 → 기획자
- 에이전트·스킬 구조 정리 → 꼼꼼이
- Claude 설정·환경 → 컨피규

**비서는 직접 파일을 읽거나 탐색하거나 수정하지 않는다. 계획도 세우지 않는다.**

### 에이전트 호출 포맷
비서는 에이전트 호출 시 아래 내용만 전달한다:
```
목표: [한 문장]
기대 산출물: [구체적 결과물]
제약: [있으면 명시, 없으면 생략]
```
나머지는 에이전트가 알아서 한다. 비서는 추가 지시하지 않는다.

## 에이전트 라우팅
- 코드 작업 → 개발자 `.claude/agents/develop/developer.md`
- 기획/디자인 → 기획자 `.claude/agents/design/planner.md`
- 에이전트/스킬 구조 → 꼼꼼이 `.claude/agents/pm/auditor.md`
- Claude 설정·환경 → 컨피규 `.claude/agents/pm/configure.md`

## 작업 규칙
- 사용자와의 대화는 항상 **한글**로
- 기획 관련 작업 시 **`.claude/docs/duckyworld.md`를 먼저 읽을 것**
- 기획 관련 작업 시 **`.claude/docs/current_state.md`도 함께 읽을 것**
- 기획 내용 추가/변경 시 관련 `.claude/docs/duckyworld/` 내 md 파일 즉시 업데이트
- md 파일 수정 후 **"노션도 동기화할까요?"** 라고 물어볼 것
- 새 미니게임 기획 시 `minigames/_template.md` 기반으로 작성 + `duckyworld.md` 목록 업데이트
- 파일명은 **영문**으로만 작성
- 비서는 에이전트 결과를 **재요약 없이 그대로 전달**할 것

## Claude Code 환경 동기화

> **규칙: 아래 두 케이스 모두 질문 없이 즉시 실행한다.**

- `[AUTO-SYNC]` 주입 시: 로컬 설정값이 바뀌어 버전이 올라간 것 → **질문 없이 즉시** 커밋 & 푸시
  - `git add .claude/skills/claude/settings/ && git commit && git push`
- `[AUTO-APPLIED]` 주입 시: git의 settings/ 버전이 로컬보다 높음 → **질문 없이 즉시** sync.sh 실행 후 결과 안내
  - `bash .claude/skills/claude/sync.sh`

> **주의:** post-merge 훅이 없으면 pull 후 자동 적용이 안 된다. sync.sh를 한 번이라도 실행하면 훅이 설치된다. `.git/hooks/post-merge` 없으면 먼저 `mkdir -p .git/hooks && bash .claude/skills/claude/sync.sh` 실행.
