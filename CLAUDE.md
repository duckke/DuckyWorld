# DuckyWorld - Claude Instructions

## 프로젝트 개요
- **게임명**: DuckyWorld — 캐주얼 미니게임 컬렉션
- **엔진**: Unity (URP) / Firebase / Photon PUN2
- **플랫폼**: iOS / Android / PC
- **상태**: 기획 단계 (개발 미착수)

## 에이전트 사용 규칙

**아래 중 하나라도 해당하면 반드시 오케스트레이터를 통해 수행한다. 예외 없음.**
- 파일을 Read로 읽는 행위가 포함되는 경우
- 파일을 수정/생성/삭제하는 경우
- 경로/참조를 탐색하거나 변경하는 경우

**오케스트레이터 라우팅:**
- 코드 작업 → `.claude/agents/develop/orchestrator.md`
- 기획/디자인 → `.claude/agents/design/orchestrator.md`
- Notion 동기화 → `.claude/agents/notion/orchestrator.md`
- Claude 환경(스킬/에이전트/설정/구조) → `.claude/agents/claude/orchestrator.md`

**메인컨텍스트가 직접 해도 되는 것:**
- git 명령 (status, add, commit, push 등)
- 사용자에게 답변/설명
- 오케스트레이터 호출

공통 유틸 에이전트: `.claude/agents/common/` (explorer, planner, modifier)

## 스킬 작성 규칙
- 스킬에 규칙 추가 시 일반화된 내용으로 작성 — 지나치게 구체적인 예시 금지

## 작업 규칙
- 사용자와의 대화는 항상 **한글**로
- 기획 관련 작업 시 **`.claude/docs/duckyworld.md`를 먼저 읽을 것** — 전체 구조와 문서 트리
- 기획 내용 추가/변경 시 관련 `.claude/docs/duckyworld/` 내 md 파일 **즉시 업데이트**
- md 파일 수정 후 **"노션도 동기화할까요?"** 라고 물어볼 것
- 새 미니게임 기획 시 `minigames/_template.md` 기반으로 작성 + `duckyworld.md` 목록 업데이트
- 파일명은 **영문**으로만 작성

## Claude Code 환경 동기화
- `[AUTO-SYNC]` 주입 시: 사용자에게 커밋 여부 질문 → 동의 시 커밋 & 푸시
- `[AUTO-APPLIED]` 주입 시: settings/ 버전이 로컬보다 높음 → 즉시 sync.sh 실행 후 결과 안내
