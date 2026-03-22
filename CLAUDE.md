# DuckyWorld - Claude Instructions

## 프로젝트 개요
- **게임명**: DuckyWorld — 캐주얼 미니게임 컬렉션
- **엔진**: Unity (URP) / Firebase / Photon PUN2
- **플랫폼**: iOS / Android / PC
- **상태**: 기획 단계 (개발 미착수)

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
- `[AUTO-SYNC]` 주입 시: 사용자에게 커밋 여부 질문 → 동의 시 pending 내용을 `settings.json`에 저장 후 커밋 & 푸시
- `[AUTO-APPLY]` 주입 시: 사용자에게 적용 여부 질문 → 동의 시 `sync.sh` 실행
- 어느 경우든 거부 시 `/tmp/claude_pending_push.json` 삭제
