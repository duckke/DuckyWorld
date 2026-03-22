# DuckyWorld - Claude Instructions

## 프로젝트 개요
- **게임명**: DuckyWorld — 캐주얼 미니게임 컬렉션
- **엔진**: Unity (URP) / Firebase / Photon PUN2
- **플랫폼**: iOS / Android / PC
- **상태**: 기획 단계 (개발 미착수)

## 조직 구조

```
대표 (사용자)
  └── 비서 (메인컨텍스트) — 지시 전달 및 결과 보고만 담당
        └── 개발팀장 → 코더, 리뷰어, 리팩터, 뒤적이, 뚝딱이
        └── 기획팀장 → 크리, 로직, 뒤적이, 뚝딱이
        └── 노션팀장 → 다듬이, 뒤적이
        └── 클로드팀장 → 뒤적이, 뚝딱이
```

**비서(메인컨텍스트) 역할:**
- 대표의 지시를 받아 적절한 팀장에게 전달한다
- 팀장의 결과를 받아 대표에게 보고한다
- 직접 파일을 읽거나 수정하지 않는다 — 모든 실행은 팀장에게 위임
- 직접 해도 되는 것: git 명령, 대표에게 답변/보고, 팀장 호출

**팀장 라우팅:**
- 코드 작업 → 개발팀장 `.claude/agents/develop/orchestrator.md`
- 기획/디자인 → 기획팀장 `.claude/agents/design/orchestrator.md`
- Notion 동기화 → 노션팀장 `.claude/agents/notion/orchestrator.md`
- Claude 환경(스킬/에이전트/설정) → 클로드팀장 `.claude/agents/claude/orchestrator.md`

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
